//  Copyright 2005 University of Wisconsin
//  Authors:
//      Robert M. Scheller
//      James B. Domingo
//  BDA originally programmed by Wei (Vera) Li at University of Missouri-Columbia in 2004.
//  Version 1.0
//  License:  Available at
//  http://landis.forest.wisc.edu/developers/LANDIS-IISourceCodeLicenseAgreement.pdf

using System.Collections.Generic;
using Edu.Wisc.Forest.Flel.Util;

namespace Landis.BDA
{
    /// <summary>
    /// Parameters for the extension.
    /// </summary>
    public interface IInputParameters
    {
        /// <summary>
        /// Timestep (years)
        /// </summary>
        int Timestep {get;set;}
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for output maps.
        /// </summary>
        string MapNamesTemplate{get;set;}
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for output SRD maps.
        /// </summary>
        string SRDMapNames{get;set;}
        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for output SRD maps.
        /// </summary>
        string NRDMapNames{get;set;}
        //---------------------------------------------------------------------
        /// <summary>
        /// Name of log file.
        /// </summary>
        string LogFileName{get;set;}

        //---------------------------------------------------------------------
        /// <summary>
        /// List of Agent Files
        /// </summary>
        IEnumerable<IAgent> ManyAgentParameters{get;set;}
    }
}

namespace Landis.BDA
{
    /// <summary>
    /// Parameters for the plug-in.
    /// </summary>
    public class InputParameters
        : IInputParameters
    {
        private int timestep;
        private string mapNamesTemplate;
        private string srdMapNames;
        private string nrdMapNames;
        private string logFileName;
        private IEnumerable<IAgent> manyAgentParameters;

        //---------------------------------------------------------------------
        /// <summary>
        /// Timestep (years)
        /// </summary>
        public int Timestep
        {
            get {
                return timestep;
            }
            set {
                if (value < 0)
                        throw new InputValueException(value.ToString(),
                                                      "Value must be = or > 0.");
                timestep = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for output maps.
        /// </summary>
        public string MapNamesTemplate
        {
            get {
                return mapNamesTemplate;
            }
            set {
                MapNames.CheckTemplateVars(value);
                mapNamesTemplate = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for SRD output maps.
        /// </summary>
        public string SRDMapNames
        {
            get
            {
                return srdMapNames;
            }
            set
            {
                MapNames.CheckTemplateVars(value);
                srdMapNames = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Template for the filenames for SRD output maps.
        /// </summary>
        public string NRDMapNames
        {
            get
            {
                return nrdMapNames;
            }
            set
            {
                MapNames.CheckTemplateVars(value);
                nrdMapNames = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// Name of log file.
        /// </summary>
        public string LogFileName
        {
            get {
                return logFileName;
            }
            set {
                    // FIXME: check for null or empty path (value.Actual);
                logFileName = value;
            }
        }

        //---------------------------------------------------------------------
        /// <summary>
        /// List of Agent Files
        /// </summary>
        public IEnumerable<IAgent> ManyAgentParameters
        {
            get {
                return manyAgentParameters;
            }
            set {
                manyAgentParameters = value;
            }
        }

        //---------------------------------------------------------------------
        public InputParameters()
        {
        }
        //---------------------------------------------------------------------
/*        public Parameters(int                timestep,
                          string             mapNameTemplate,
                          string             srdMapNames,
                          string  nrdMapNames,
                          string             logFileName,
                          IEnumerable<IAgent>   manyAgentParameters
                          )
        {
            this.timestep = timestep;
            this.mapNamesTemplate = mapNameTemplate;
            this.srdMapNames = srdMapNames;
            this.nrdMapNames = nrdMapNames;
            this.logFileName = logFileName;
            this.manyAgentParameters = manyAgentParameters;
        }*/
    }
}
