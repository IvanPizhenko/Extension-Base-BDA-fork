//  Copyright 2005-2010 Portland State University, University of Wisconsin
//  Authors:  Robert M. Scheller,   James B. Domingo

using Landis.Core;
using Landis.Library.AgeOnlyCohorts;
using Landis.SpatialModeling;

using System.Collections.Generic;
using System.Linq;

namespace Landis.Extension.BaseBDA
{
    public class Epidemic
        : ICohortDisturbance

    {
        public class KillResult
        {
            public readonly int CohortsKilled;
            public readonly int CFSConifersKilled;
            public readonly long BiomassKilled;

            public KillResult(int cohortsKilled, int cfsConifersKilled, long biomassKilled)
            {
                CohortsKilled = cohortsKilled;
                CFSConifersKilled = cfsConifersKilled;
                BiomassKilled = biomassKilled;
            }
        }

        private static IEcoregionDataset _ecoregions;
        private IAgent _epidemicParms;
        private int _totalSitesDamaged;
        private int _totalCohortsKilled;
        private double _meanSeverity;
        private long _totalBiomassKilled;
        private int _siteSeverity;
        private double _random;
        private double _siteVulnerability;
        //private int _advRegenAgeCutoff;
        private int _siteCohortsKilled;
        private int _siteCFSConifersKilled;
        private long _biomassKilled;
        private long _biomassCohortCount;
        private int[] _sitesInEvent;

        private ActiveSite _currentSite; // current site where cohorts are being damaged

        private enum TempPattern        {random, cyclic};
        private enum NeighborShape      {uniform, linear, gaussian};
        private enum InitialCondition   {map, none};
        private enum SRDMode            {SRDmax, SRDmean};


        //---------------------------------------------------------------------

        public int[] SitesInEvent
        {
            get {
                return _sitesInEvent;
            }
        }

        //---------------------------------------------------------------------

        public int CohortsKilled
        {
            get {
                return _totalCohortsKilled;
            }
        }

        //---------------------------------------------------------------------

        public long TotalBiomassKilled
        {
            get
            {
                return _totalBiomassKilled;
            }
        }

        //---------------------------------------------------------------------

        public double MeanSeverity
        {
            get {
                return _meanSeverity;
            }
        }

        //---------------------------------------------------------------------

        public int TotalSitesDamaged
        {
            get {
                return _totalSitesDamaged;
            }
        }
        //---------------------------------------------------------------------

        ExtensionType IDisturbance.Type
        {
            get {
                return PlugIn.type;
            }
        }

        //---------------------------------------------------------------------

        ActiveSite IDisturbance.CurrentSite
        {
            get {
                return _currentSite;
            }
        }
        //---------------------------------------------------------------------

        IAgent EpidemicParameters
        {
            get
            {
                return _epidemicParms;
            }
        }

        //---------------------------------------------------------------------
        ///<summary>
        ///Initialize an Epidemic - defined as an agent outbreak for an entire landscape
        ///at a single BDA timestep.  One epidemic per agent per BDA timestep
        ///</summary>

        public static void Initialize(IAgent agent)
        {
            PlugIn.ModelCore.UI.WriteLine("   Initializing agent {0}.", agent.AgentName);

            _ecoregions = PlugIn.ModelCore.Ecoregions;


            //.ActiveSiteValues allows you to reset all active site at once.
            SiteVars.NeighborResourceDom.ActiveSiteValues = 0;
            SiteVars.Vulnerability.ActiveSiteValues = 0;
            SiteVars.SiteResourceDomMod.ActiveSiteValues = 0;
            SiteVars.SiteResourceDom.ActiveSiteValues = 0;

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                if(agent.OutbreakZone[site] == Zone.Newzone)
                    agent.OutbreakZone[site] = Zone.Lastzone;
                else
                    agent.OutbreakZone[site] = Zone.Nozone;
            }

        }

        //---------------------------------------------------------------------
        ///<summary>
        ///Simulate an Epidemic - This is the controlling function that calls the
        ///subsequent function.  The basic logic of an epidemic resides here.
        ///</summary>
        public static Epidemic Simulate(IAgent agent,
                                        int currentTime,
                                        int timestep,
                                        int ROS)
        {


            Epidemic CurrentEpidemic = new Epidemic(agent);
            PlugIn.ModelCore.UI.WriteLine("   New BDA Epidemic Activated.");

            //SiteResources.SiteResourceDominance(agent, ROS, SiteVars.Cohorts);
            SiteResources.SiteResourceDominance(agent, ROS);
            SiteResources.SiteResourceDominanceModifier(agent);

            if(agent.Dispersal) {
                //Asynchronous - Simulate Agent Dispersal

                // Calculate Site Vulnerability without considering the Neighborhood
                // If neither disturbance modifiers nor ecoregion modifiers are active,
                //  Vulnerability will equal SiteReourceDominance.
                SiteResources.SiteVulnerability(agent, ROS, false);

                Epicenters.NewEpicenters(agent, timestep);

            } else
            {
                //Synchronous:  assume that all Active sites can potentially be
                //disturbed without regard to initial locations.
                foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
                    agent.OutbreakZone[site] = Zone.Newzone;

            }

            //Consider the Neighborhood if requested:
            if (agent.NeighborFlag)
                SiteResources.NeighborResourceDominance(agent);

            //Recalculate Site Vulnerability considering neighbors if necessary:
            SiteResources.SiteVulnerability(agent, ROS, agent.NeighborFlag);

            CurrentEpidemic.DisturbSites(agent);

            return CurrentEpidemic;
        }

        //---------------------------------------------------------------------
        // Epidemic Constructor
        private Epidemic(IAgent agent)
        {
            _sitesInEvent = new int[_ecoregions.Count];
            foreach(IEcoregion ecoregion in _ecoregions)
                _sitesInEvent[ecoregion.Index] = 0;
            _epidemicParms = agent;
            _totalCohortsKilled = 0;
            _meanSeverity = 0.0;
            _totalSitesDamaged = 0;
            _biomassCohortCount = 0;

            //PlugIn.ModelCore.Log.WriteLine("New Agent event");
        }

        //---------------------------------------------------------------------
        //Go through all active sites and damage them according to the
        //Site Vulnerability.
        private void DisturbSites(IAgent agent)
        {
            _totalBiomassKilled = 0;
            int totalSiteSeverity = 0;
            //this.advRegenAgeCutoff = agent.AdvRegenAgeCutoff;

            foreach (ActiveSite site in PlugIn.ModelCore.Landscape)
            {
                _siteSeverity = 0;
                _random = 0;

                double myRand = PlugIn.ModelCore.GenerateUniform();

                if(agent.OutbreakZone[site] == Zone.Newzone
                    && SiteVars.Vulnerability[site] > myRand)
                {
                    //PlugIn.ModelCore.Log.WriteLine("Zone={0}, agent.OutbreakZone={1}", Zone.Newzone.ToString(), agent.OutbreakZone[site]);
                    //PlugIn.ModelCore.Log.WriteLine("Vulnerability={0}, Randnum={1}", SiteVars.Vulnerability[site], PlugIn.ModelCore.GenerateUniform());
                    double vulnerability = SiteVars.Vulnerability[site];
                    if(vulnerability >= 0) _siteSeverity= 1;
                    if(vulnerability >= agent.Class2_SV) _siteSeverity= 2;
                    if(vulnerability >= agent.Class3_SV) _siteSeverity= 3;

                    _random = myRand;
                    _siteVulnerability = SiteVars.Vulnerability[site];

                    if (_siteSeverity > 0)
                    {
                        var killResult = KillSiteCohorts(site);

                        if (SiteVars.NumberCFSconifersKilled[site].ContainsKey(PlugIn.ModelCore.CurrentTime))
                        {
                            int prevKilled = SiteVars.NumberCFSconifersKilled[site][PlugIn.ModelCore.CurrentTime];
                            SiteVars.NumberCFSconifersKilled[site][PlugIn.ModelCore.CurrentTime] = prevKilled + killResult.CFSConifersKilled;
                        }
                        else
                        {
                            SiteVars.NumberCFSconifersKilled[site].Add(PlugIn.ModelCore.CurrentTime, killResult.CFSConifersKilled);
                        }

                        if (killResult.CohortsKilled > 0)
                        {
                            _totalCohortsKilled += killResult.CohortsKilled;
                            _totalBiomassKilled += killResult.BiomassKilled;
                            _totalSitesDamaged++;
                            totalSiteSeverity += _siteSeverity;
                            SiteVars.Disturbed[site] = true;
                            SiteVars.TimeOfLastEvent[site] = PlugIn.ModelCore.CurrentTime;
                            SiteVars.AgentName[site] = agent.AgentName;
                        }
                        else
                            _siteSeverity = 0;
                    }
                }
                agent.Severity[site] = (byte) _siteSeverity;
            }
            if (_totalSitesDamaged > 0)
                _meanSeverity = (double)totalSiteSeverity / _totalSitesDamaged;
        }

        //---------------------------------------------------------------------
        //A small helper function for going through list of cohorts at a site
        //and checking them with the filter provided by RemoveMarkedCohort(ICohort).
        private KillResult KillSiteCohorts(ActiveSite site)
        {
            _siteCohortsKilled = 0;
            _siteCFSConifersKilled = 0;
            _biomassKilled = 0;
            _biomassCohortCount = 0;
            _currentSite = site;

            var siteCohorts = SiteVars.Cohorts[site];
            siteCohorts.RemoveMarkedCohorts(this);
            PlugIn.ModelCore.UI.WriteLine($"BDA: Killed biomass {_biomassKilled} in {_biomassCohortCount} cohorts");

            return new KillResult(_siteCohortsKilled, _siteCFSConifersKilled, _biomassKilled);
        }

        //---------------------------------------------------------------------
        // MarkCohortForDeath is a filter to determine which cohorts are removed.
        // Each cohort is passed into the function and tested whether it should
        // be killed.
        bool ICohortDisturbance.MarkCohortForDeath(ICohort cohort)
        {
            //PlugIn.ModelCore.Log.WriteLine("Cohort={0}, {1}, {2}.", cohort.Species.Name, cohort.Age, cohort.Species.Index);
            
            bool killCohort = false;
           // bool advRegenSpp = false;

            ISppParameters sppParms = _epidemicParms.SppParameters[cohort.Species.Index];

            //foreach (ISpecies mySpecies in epidemicParms.AdvRegenSppList)
            //{
            //   if (cohort.Species == mySpecies)
            //   {
            //        advRegenSpp = true;
            //        break;
            //    }

            //}

            if (cohort.Age >= sppParms.ResistantHostAge)
            {
                if (_random <= _siteVulnerability * sppParms.ResistantHostVuln)
                {
                    //if (advRegenSpp && cohort.Age <= this.advRegenAgeCutoff)
                    //    killCohort = false;
                    //else
                        killCohort = true;
                }
            }

            if (cohort.Age >= sppParms.TolerantHostAge)
            {
                if (_random <= _siteVulnerability * sppParms.TolerantHostVuln)
                {
                    //if (advRegenSpp && cohort.Age <= this.advRegenAgeCutoff)
                     //   killCohort = false;
                    //else
                        killCohort = true;
                }
            }

            if (cohort.Age >= sppParms.VulnerableHostAge)
            {
                if (_random <= _siteVulnerability * sppParms.VulnerableHostVuln)
                {
                    //if (advRegenSpp && cohort.Age <= this.advRegenAgeCutoff)
                     //   killCohort = false;
                    //else
                        killCohort = true;
                }
            }
            

            if (killCohort)
            {
                _siteCohortsKilled++;
                if (sppParms.CFSConifer)
                    _siteCFSConifersKilled++;

                if (cohort is Landis.Library.BiomassCohorts.ICohort)
                {
                    var biomassCohort = cohort as Landis.Library.BiomassCohorts.ICohort;
                    _biomassKilled += biomassCohort.Biomass;
                    ++_biomassCohortCount;
                }
            }

            return killCohort;
        }
    }
}
