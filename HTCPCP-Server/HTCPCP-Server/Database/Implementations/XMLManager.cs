using HTCPCP_Server.Database.Interfaces;
using HTCPCP_Server.Enumerations;
using HTCPCP_Server.Helpers;
using HTCPCP_Server.Logging;
using System.CommandLine;
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

        /// <summary>
        /// Reads and parses a xml option file
        /// </summary>
        /// <param name="info">The file to parse</param>
        /// <returns>Returns a dictionary over all pots</returns>
        private async Task<Dictionary<string, Dictionary<Enumerations.Option, int>>?> ReadFile(FileInfo info)
        {
            if (!info.Exists)
            {
                Log.Info("File not found!");
                return null;
            }

            var dict = new Dictionary<string, Dictionary<Enumerations.Option, int>>();
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(info.FullName);

                foreach (XmlNode child in doc.ChildNodes)
                {
                    if (child.Name == "options")
                    {
                        foreach (XmlNode node in child)
                        {
                            if (node.Name == "additions")
                            {
                                var pot = node.Attributes?.GetNamedItem("pot")?.InnerText;
                                if (pot == null)
                                {
                                    Log.Info("No pot name specified!");
                                    continue;
                                }

                                var potDict = new Dictionary<Enumerations.Option, int>();
                                foreach (XmlNode add in node.ChildNodes)
                                {
                                    if (add.Name == "addition")
                                    {
                                        this.parseAddition(add, potDict);
                                    }
                                }

                                dict.Add(pot, potDict);
                            }
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                Log.Info($"File does not contain valid xml!\n{e.Message}\n@ Line {e.LineNumber} @ Position {e.LinePosition}");
                return null;
            }
            catch (UnauthorizedAccessException e)
            {
                Log.Info($"File could not be accessed!\n{e.Message}");
                return null;
            }

            return dict;
        }

        /// <summary>
        /// Parses an addition node
        /// </summary>
        /// <param name="node">The XmlNode to parse</param>
        /// <param name="dict">The dictionary to add to</param>
        /// <returns>Returns true if the node is parseable</returns>
        private bool parseAddition(XmlNode node, Dictionary<Enumerations.Option, int> dict)
        {
            if (node.Name == "addition")
            {
                var type = node.Attributes?.GetNamedItem("type")?.InnerText;
                if (type == null)
                {
                    Log.Info("No type found! Ignoring node");
                    return false;
                }

                Enumerations.Option opt;

                try
                {
                    opt = (Enumerations.Option)Enum.Parse(typeof(Enumerations.Option), type);
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
            if (file.IsReadOnly)
            {
                Log.Error("File is read-only!");
                return false;
            }

            var dict = await this.driver.GetAsDict();

            if (dict == null)
            {
                Log.Error("An error has occured - the database could not be exported!");
                return false;
            }

            return await this.WriteToFile(dict, file);
        }

        private async Task<bool> WriteToFile(Dictionary<string, Dictionary<Enumerations.Option, int>> dict, FileInfo file)
        {
            using (StreamWriter writer = new StreamWriter(file.FullName, false))
            {
                await writer.WriteLineAsync("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                await writer.WriteLineAsync("<options>");

                dict.AsEnumerable()
                    .ToList()
                    .ForEach(async pot =>
                    {
                        await writer.WriteLineAsync($"\t<additions pot=\"{pot.Key}\">");

                        pot.Value.AsEnumerable()
                            .ToList()
                            .ForEach(async option =>
                                await writer.WriteLineAsync($"\t\t<addition type=\"{option.Key}\">{option.Value}</addition>")
                            );

                        await writer.WriteLineAsync($"\t</additions>");

                    });

                await writer.WriteLineAsync("</options>");
            }

            return true;
        }
    }
}
