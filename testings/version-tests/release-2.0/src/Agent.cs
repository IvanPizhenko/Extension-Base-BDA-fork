//  Copyright 2005 University of Wisconsin
//  Authors:
//      Robert M. Scheller
//      James B. Domingo
//  Version 1.0
//  License:  Available at
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using Landis.Species;
using Landis.Landscape;
using Edu.Wisc.Forest.Flel.Util;
using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Grids;

namespace Landis.BDA
{
    public enum TemporalType {pulse,  variablepulse};
    public enum RandomFunction {RFnormal, RFuniform};
    public enum SRDmode {max, mean};
    public enum DispersalTemplate {MaxRadius, N4, N8, N12, N24};
    public enum NeighborShape {uniform, linear, gaussian};
    public enum NeighborSpeed {none, X2, X3, X4};
    public enum Zone {Nozone, Lastzone, Newzone};



    /// <summary>
    /// Interface to the Parameters for the extension
    /// </summary>
    public interface IAgent
    {
        string AgentName{get;set;}
        int BDPCalibrator{get;set;}
        int TimeSinceLastEpidemic{get;set;}
        int TimeToNextEpidemic{get;set;}
        TemporalType TempType{get;set;}
        RandomFunction RandFunc{get;set;}
        SRDmode SRDmode{get;set;}
        double RandomParameter1{get;set;}
        double RandomParameter2{get;set;}
        int MinROS{get;set;}
        int MaxROS{get;set;}
        ISppParameters[] SppParameters{get;set;}
        IEcoParameters[] EcoParameters{get;set;}
        IDistParameters[] DistParameters{get;set;}
        ISiteVar<byte> Severity{get;set;}
        ISiteVar<Zone> OutbreakZone{get;set;}


        //-- DISPERSAL -------------
        bool Dispersal{get;set;}
        int DispersalRate{get;set;}
        double EpidemicThresh{get;set;}
        int EpicenterNum{get;set;}
        bool SeedEpicenter{get;set;}
        double OutbreakEpicenterCoeff{get;set;}
        double SeedEpicenterCoeff{get;set;}
        DispersalTemplate DispersalTemp{get;set;}
        IEnumerable<RelativeLocation> DispersalNeighbors{get;set;}

        // Neighborhood Resource Dominance parameters
        bool NeighborFlag{get;set;}
        NeighborSpeed NeighborSpeedUp{get;set;}
        int NeighborRadius{get; set;}
        NeighborShape ShapeOfNeighbor{get;set;}
        double NeighborWeight{get;set;}
        IEnumerable<RelativeLocationWeighted> ResourceNeighbors{get;set;}
        IEnumerable<ISpecies> NegSppList { get;set;}
    }
}


namespace Landis.BDA
{
    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public class Agent
        : IAgent
    {
        private string agentName;
        private int bdpCalibrator;
        private int timeSinceLastEpidemic;
        private int timeToNextEpidemic;
        private TemporalType tempType;
        private RandomFunction randFunc;
        private SRDmode srdMode;
        private double randomParameter1;
        private double randomParameter2;
        private int minROS;
        private int maxROS;
        private ISppParameters[] sppParameters;
        private IEcoParameters[] ecoParameters;
        private IDistParameters[] distParameters;
        private ISiteVar<byte> severity;
        private ISiteVar<Zone> outbreakZone;

        //-- DISPERSAL -------------
        private bool dispersal;
        private int dispersalRate;
        private double epidemicThresh;
        private int epicenterNum;
        private bool seedEpicenter;
        private double outbreakEpicenterCoeff;
        private double seedEpicenterCoeff;
        private DispersalTemplate dispersalTemp;
        private IEnumerable<RelativeLocation> dispersalNeighbors;
        //private ArrayList dispersalNeighbors;

        // Neighborhood Resource Dominance parameters
        private bool neighborFlag;
        private NeighborSpeed neighborSpeedUp;
        private int neighborRadius;
        private NeighborShape shapeOfNeighbor;
        private double neighborWeight;
        private IEnumerable<RelativeLocationWeighted> resourceNeighbors;

        private IEnumerable<ISpecies> negSppList;

        //---------------------------------------------------------------------
        public string AgentName
        {
            get {
                return agentName;
            }
            set {
                agentName = value;
            }
        }
        //---------------------------------------------------------------------
        public int BDPCalibrator
        {
            get {
                return bdpCalibrator;
            }
            set {
                bdpCalibrator = value;
            }
        }
        //---------------------------------------------------------------------
        public int TimeSinceLastEpidemic
        {
            get {
                return timeSinceLastEpidemic;
            }
            set {
                if (value < 0)
                        throw new InputValueException(value.ToString(),
                            "Value must = or be > 0.");
                if (value > 1000)
                        throw new InputValueException(value.ToString(),
                            "Value must < 1000.");
                timeSinceLastEpidemic = value;
            }
        }
        //---------------------------------------------------------------------
        public int TimeToNextEpidemic
        {
            get {
                return timeToNextEpidemic;
            }
            set {
                timeToNextEpidemic = value;
            }
        }
        //---------------------------------------------------------------------
        public TemporalType TempType
        {
            get {
                return tempType;
            }
            set {
                tempType = value;
            }
        }
        //---------------------------------------------------------------------
        public RandomFunction RandFunc
        {
            get {
                return randFunc;
            }
            set {
                randFunc = value;
            }
        }
        //---------------------------------------------------------------------
        public SRDmode SRDmode
        {
            get {
                return srdMode;
            }
            set {
                srdMode = value;
            }
        }
        //---------------------------------------------------------------------
        public double RandomParameter1
        {
            get {
                return randomParameter1;
            }
            set {
                randomParameter1 = value;
            }
        }
        //---------------------------------------------------------------------
        public double RandomParameter2
        {
            get {
                return randomParameter2;
            }
            set {
                randomParameter2 = value;
            }
        }
        //---------------------------------------------------------------------
        public int MinROS
        {
            get {
                return minROS;
            }
            set {
                if (value < 0)
                        throw new InputValueException(value.ToString(),
                            "Value must = or be > 0.");
                if (maxROS > 0 && value > maxROS)
                        throw new InputValueException(value.ToString(),
                            "Value must < or = MaxROS.");

                minROS = value;
            }
        }
        //---------------------------------------------------------------------
        public int MaxROS
        {
            get {
                return maxROS;
            }
            set {
                if (value < 0)
                        throw new InputValueException(value.ToString(),
                            "Value must = or be > 0.");
                if (minROS > 0 && value < minROS)
                        throw new InputValueException(value.ToString(),
                            "Value must > or = MinROS.");
                maxROS = value;
            }
        }
        //---------------------------------------------------------------------
        public ISppParameters[] SppParameters
        {
            get {
                return sppParameters;
            }
            set {
                sppParameters = value;
            }
        }
        //---------------------------------------------------------------------
        public IEcoParameters[] EcoParameters
        {
            get {
                return ecoParameters;
            }
            set {
                ecoParameters = value;
            }
        }
        //---------------------------------------------------------------------
        public IDistParameters[] DistParameters
        {
            get {
                return distParameters;
            }
            set {
                distParameters = value;
            }
        }
        //---------------------------------------------------------------------
        public ISiteVar<byte> Severity
        {
            get {
                return severity;
            }
            set {
                severity = value;
            }
        }
        public ISiteVar<Zone> OutbreakZone
        {
            get {
                return outbreakZone;
            }
            set {
                outbreakZone = value;
            }
        }

        //---------------------------------------------------------------------
        public bool Dispersal
        {
            get {
                return dispersal;
            }
            set {
                dispersal = value;
            }
        }
        //---------------------------------------------------------------------
        public int DispersalRate
        {
            get {
                return dispersalRate;
            }
            set {
                if (value <= 0)
                        throw new InputValueException(value.ToString(),
                            "Value must be > 0.");
                dispersalRate = value;
            }
        }
        //---------------------------------------------------------------------
        public double EpidemicThresh
        {
            get {
                return epidemicThresh;
            }
            set {
                if (value < 0.0 || value > 1.0)
                       throw new InputValueException(value.ToString(),
                            "Value must be > or = 0 and < or = 1.");
                epidemicThresh = value;
            }
        }
        //---------------------------------------------------------------------
        public int EpicenterNum
        {
            get {
                return epicenterNum;
            }
            set {
                epicenterNum = value;
            }
        }
        //---------------------------------------------------------------------
        public bool SeedEpicenter
        {
            get {
                return seedEpicenter;
            }
            set {
                seedEpicenter = value;
            }
        }
        //---------------------------------------------------------------------
        public double OutbreakEpicenterCoeff
        {
            get {
                return outbreakEpicenterCoeff;
            }
            set {
                outbreakEpicenterCoeff = value;
            }
        }
        //---------------------------------------------------------------------
        public double SeedEpicenterCoeff
        {
            get {
                return seedEpicenterCoeff;
            }
            set {
                seedEpicenterCoeff = value;
            }
        }
        //---------------------------------------------------------------------
        public DispersalTemplate DispersalTemp
        {
            get {
                return dispersalTemp;
            }
            set {
                dispersalTemp = value;
            }
        }
        //---------------------------------------------------------------------
        public IEnumerable<RelativeLocation> DispersalNeighbors
        {
            get {
                return dispersalNeighbors;
            }
            set {
                dispersalNeighbors = value;
            }
        }
        //---------------------------------------------------------------------
        public bool NeighborFlag
        {
            get {
                return neighborFlag;
            }
            set {
                neighborFlag = value;
            }
        }
        //---------------------------------------------------------------------
        public NeighborSpeed NeighborSpeedUp
        {
            get {
                return neighborSpeedUp;
            }
            set {
                 neighborSpeedUp = value;
            }
        }
        //---------------------------------------------------------------------
        public int NeighborRadius
        {
            get {
                return neighborRadius;
            }
            set {
                if (value <= 0)
                        throw new InputValueException(value.ToString(),
                            "Value must be > 0.");
                neighborRadius = value;
            }
        }
        //---------------------------------------------------------------------
        public NeighborShape ShapeOfNeighbor
        {
            get {
                return shapeOfNeighbor;
            }
            set {
                shapeOfNeighbor = value;
            }
        }
        //---------------------------------------------------------------------
        public double NeighborWeight
        {
            get {
                return neighborWeight;
            }
            set {
                if (value < 0)
                        throw new InputValueException(value.ToString(),
                            "Value must = or be > 0.");
                neighborWeight = value;
            }
        }
        //---------------------------------------------------------------------
        public IEnumerable<RelativeLocationWeighted> ResourceNeighbors
        {
            get {
                return resourceNeighbors;
            }
            set {
                resourceNeighbors = value;
            }
        }
        //---------------------------------------------------------------------
        public IEnumerable<ISpecies> NegSppList
        {
            get
            {
                return negSppList;
            }
            set
            {
                negSppList = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Objects and Lists must be initialized.
        /// </summary>
        public Agent(int sppCount, int ecoCount, int distCount)
        {
            SppParameters = new ISppParameters[sppCount];
            EcoParameters = new IEcoParameters[ecoCount];
            DistParameters = new IDistParameters[distCount];
            negSppList = new List<ISpecies>();
            dispersalNeighbors = new List<RelativeLocation>();
            resourceNeighbors = new List<RelativeLocationWeighted>();
            severity       = Model.Core.Landscape.NewSiteVar<byte>();
            outbreakZone   = Model.Core.Landscape.NewSiteVar<Zone>();

            for(int i=0; i<sppCount; i++)
                SppParameters[i] = new SppParameters();
        }



    }

    public class RelativeLocationWeighted
    {
        private RelativeLocation location;
        private double weight;

        //---------------------------------------------------------------------
        public RelativeLocation Location
        {
            get {
                return location;
            }
            set {
                location = value;
            }
        }

        public double Weight
        {
            get {
                return weight;
            }
            set {
                weight = value;
            }
        }

        public RelativeLocationWeighted (RelativeLocation location, double weight)
        {
            this.location = location;
            this.weight = weight;
        }

    }
}
