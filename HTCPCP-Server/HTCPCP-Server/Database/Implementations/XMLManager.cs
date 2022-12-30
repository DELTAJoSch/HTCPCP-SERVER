using HTCPCP_Server.Database.Interfaces;
using HTCPCP_Server.Enumerations;
using HTCPCP_Server.Helpers;
using HTCPCP_Server.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HTCPCP_Server.Database.Implementations
{
    /// <summary>
    /// A Database Manager for XML files
    /// </summary>
    [FileExtension(".xml")]
    [FileExtension(".XML")]
    internal class XMLManager : IDatabaseManager
    {
        private IDatabaseDriver driver;

        /// <summary>
        /// Creates an xml manager with the given database driver
        /// </summary>
        /// <param name="driver">The driver to use when creating</param>
        public XMLManager(IDatabaseDriver driver)
        {
            this.driver = driver;
        }

        /// <summary>
        /// Loads an xml file into the database
        /// Replaces existing content of the database
        /// </summary>
        /// <param name="file">The file to Load</param>
        /// <returns>Returns true if successful</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Load(FileInfo file)
        {
            var dict = await this.ReadFile(file);

            if (dict == null)
                return false;

            return await this.driver.InitDb(dict);
        }

        private async Task<Dictionary<string, Dictionary<Option, int>>?> ReadFile(FileInfo info) 
        {
            if (!info.Exists) 
            {
                Log.Info("File not found!");
                return null;
            }

            var dict = new Dictionary<string, Dictionary<Option, int>>();
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(info.FullName);

                foreach (XmlNode node in doc.ChildNodes)
                {
                    if (node.Name == "additions") 
                    {
                        var pot = node.Attributes?.GetNamedItem("pot")?.InnerText;
                        if (pot == null) 
                        {
                            Log.Info("No pot name specified!");
                            continue;                        
                        }

                        var potDict = new Dictionary<Option, int>();
                        foreach(var add in node.ChildNodes) 
                        {
                            if (node.Name == "addition")
                            {
                                this.parseAddition(node, potDict);
                            }
                        }

                        dict.Add(pot, potDict);
                    }
                }
            }
            catch (XmlException e)
            {
                Log.Info($"File does not contain valid xml!\n{e.Message}\n@ Line {e.LineNumber} @ Position {e.LinePosition}");
                return null;
            }
            catch(UnauthorizedAccessException e)
            {
                Log.Info($"File could not be accessed!\n{e.Message}");
                return null;
            }

            return dict;
        }


        private bool parseAddition(XmlNode node, Dictionary<Option, int> dict) 
        {
            if (node.Name == "addition")
            {
                var type = node.Attributes?.GetNamedItem("type")?.InnerText;
                if (type == null)
                {
                    Log.Info("No type found! Ignoring node");
                    return false;
                }

                Option opt;

                try
                {
                    opt = (Option)Enum.Parse(typeof(Option), type);
                }
                catch (ArgumentException)
                {
                    Log.Info($"Unknown type {type}");
                    return false;
                }

                int count;
                if (!int.TryParse(node.InnerText, out count))
                {
                    Log.Info($"NaN: {node.InnerText}");
                    return false;
                }

                dict.Add(opt, count);
            }
            else
            {
                Log.Info($"Unexpected Node: {node.Name}");
            }
            return true;        
        }

        /// <summary>
        /// Creates an xml file from the database
        /// </summary>
        /// <param name="file">The file to write to</param>
        /// <returns>Returns true if successful</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<bool> Save(FileInfo file)
        {
            throw new NotImplementedException();
        }
    }
}
