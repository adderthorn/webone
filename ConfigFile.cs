﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using static WebOne.Program;

namespace WebOne
{
	/// <summary>
	/// Config file entries and parser
	/// </summary>
	static class ConfigFile
	{
		private static LogWriter Log = new LogWriter();
		static string ConfigFileName = Program.ConfigFileName;
		static List<string> StringListConstructor = new List<string>();
		static List<List<string>> RawEditSets = new List<List<string>>();
		static int LastRawEditSet = -1;

		static string[] SpecialSections = { "ForceHttps", "TextTypes", "ForceUtf8", "InternalRedirectOn", "Converters" };

		/// <summary>
		/// TCP port that should be used by the Proxy Server
		/// </summary>
		public static int Port = 80;

		/// <summary>
		/// List of domains that should be open only using HTTPS
		/// </summary>
		public static string[] ForceHttps = { "www.phantom.sannata.org.example" };

		/// <summary>
		/// List of URLs that should be always downloaded as UTF-8
		/// </summary>
		public static string[] ForceUtf8 = { "yandex.ru.example" };

		/// <summary>
		/// List of parts of Content-Types that describing text files
		/// </summary>
		public static string[] TextTypes = { "text/", "javascript"};

		/// <summary>
		/// Encoding to be used in output content
		/// </summary>
		public static Encoding OutputEncoding = Encoding.Default;

		/// <summary>
		/// Credentials for proxy authentication
		/// </summary>
		public static string Authenticate = "";

		/// <summary>
		/// (Legacy) List of URLs that should be always 302ed
		/// </summary>
		public static List<string> FixableURLs = new List<string>();

		/// <summary>
		/// (Legacy) Dictionary of URLs that should be always 302ed if they're looks like too new JS frameworks
		/// </summary>
		public static Dictionary<string, Dictionary<string, string>> FixableUrlActions =  new Dictionary<string, Dictionary<string, string>>();

		/// <summary>
		/// (Legacy) List of Content-Types that should be always 302ed
		/// </summary>
		public static List<string> FixableTypes = new List<string>();

		/// <summary>
		/// (Legacy) Dictionary of Content-Types that should be always 302ed to converter
		/// </summary>
		public static Dictionary<string, Dictionary<string, string>> FixableTypesActions = new Dictionary<string, Dictionary<string, string>>();

		/// <summary>
		/// (Legacy) List of possible content patches
		/// </summary>
		public static List<string> ContentPatches = new List<string>();

		/// <summary>
		/// (Legacy) Dictionary of possible content patches
		/// </summary>
		public static Dictionary<string, Dictionary<string, string>> ContentPatchActions = new Dictionary<string, Dictionary<string, string>>();

		/// <summary>
		/// List of domains where 302 redirections should be passed through .NET FW
		/// </summary>
		public static string[] InternalRedirectOn = { "flickr.com.example", "www.flickr.com.example"};

		/// <summary>
		/// Hide "Can't read from client" and "Cannot return reply to the client" error messages in log
		/// </summary>
		public static bool HideClientErrors = false;

		/// <summary>
		/// Search for copies of removed sites in web.archive.org
		/// </summary>
		public static bool SearchInArchive = false;

		/// <summary>
		/// Make Web.Archive.Org error messages laconic (for retro browsers)
		/// </summary>
		public static bool ShortenArchiveErrors = false;

		/// <summary>
		/// List of enabled file format converters
		/// </summary>
		public static List<Converter> Converters = new List<Converter>();

		/// <summary>
		/// User-agent string of the Proxy
		/// </summary>
		public static string UserAgent = "%Original% WebOne/%WOVer%";

		/// <summary>
		/// Proxy default host name (or IP)
		/// </summary>
		public static string DefaultHostName = Environment.MachineName;

		/// <summary>
		/// Break network operations when remote TLS certificate is bad
		/// </summary>
		public static bool ValidateCertificates = true;

		/// <summary>
		/// List of possible traffic editing rule sets
		/// </summary>
		public static List<EditSet> EditRules = new List<EditSet>();

		/// <summary>
		/// Table for alphabet transliteration
		/// </summary>
		public static List<KeyValuePair<string, string>> TranslitTable = new List<KeyValuePair<string, string>>();

		/// <summary>
		/// Temporary files' directory
		/// </summary>
		public static string TemporaryDirectory = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar;

		/// <summary>
		/// Allow or disallow clients see webone.conf content
		/// </summary>
		public static bool AllowConfigFileDisplay = true;

		/// <summary>
		/// Set status page display style: no, short, full
		/// </summary>
		public static string DisplayStatusPage = "full";

		static ConfigFile()
		{
			//ConfigFileName = "webone.conf";
			Console.WriteLine("Using configuration file {0}.", ConfigFileName);
			int i = 0;
			try
			{
				if (!File.Exists(ConfigFileName)) { Log.WriteLine(true, false, "{0}: no such config file. Using defaults.", ConfigFileName); return; };
				
				string[] CfgFile = System.IO.File.ReadAllLines(ConfigFileName);
				string Section = "";
				for (i = 0; i < CfgFile.Count(); i++)
				{
					try
					{
						if (CfgFile[i] == "") continue; //empty lines
						if (CfgFile[i].StartsWith(";")) continue; //comments
						if (CfgFile[i].StartsWith("[")) //section
						{
							Section = CfgFile[i].Substring(1, CfgFile[i].Length - 2);
							StringListConstructor.Clear();

							if (Section.StartsWith("FixableURL:"))
							{
								FixableURLs.Add(Section.Substring(11));
								FixableUrlActions.Add(Section.Substring(11), new Dictionary<string, string>());
							}

							if (Section.StartsWith("FixableType:"))
							{
								FixableTypes.Add(Section.Substring(12));
								FixableTypesActions.Add(Section.Substring(12), new Dictionary<string, string>());
							}

							if (Section.StartsWith("ContentPatch:"))
							{
								ContentPatches.Add(Section.Substring(13));
								ContentPatchActions.Add(Section.Substring(13), new Dictionary<string, string>());
							}

							if (Section.StartsWith("ContentPatchFind:"))
							{
								Log.WriteLine(true, false, "Warning: ContentPatchFind sections are no longer supported. See wiki.");
							}

							if (Section.StartsWith("Edit:"))
							{
								LastRawEditSet++;
								RawEditSets.Add(new List<string>());
								RawEditSets[LastRawEditSet].Add("OnUrl=" + Section.Substring("Edit:".Length));
							}

							if (Section == "Edit")
							{
								LastRawEditSet++;
								RawEditSets.Add(new List<string>());
							}

							continue;
						}


						//Log.WriteLine(true, false, Section);
						if (Program.CheckString(Section, SpecialSections)) //special sections (patterns, lists, etc)
						{
							//Log.WriteLine(true, false, "{0}+={1}", Section, CfgFile[i]);
							switch (Section)
							{
								case "ForceHttps":
									StringListConstructor.Add(CfgFile[i]);
									ForceHttps = StringListConstructor.ToArray();
									continue;
								case "TextTypes":
									StringListConstructor.Add(CfgFile[i]);
									TextTypes = StringListConstructor.ToArray();
									continue;
								case "ForceUtf8":
									StringListConstructor.Add(CfgFile[i]);
									ForceUtf8 = StringListConstructor.ToArray();
									continue;
								case "InternalRedirectOn":
									StringListConstructor.Add(CfgFile[i]);
									InternalRedirectOn = StringListConstructor.ToArray();
									continue;
								case "Converters":
									Converters.Add(new Converter(CfgFile[i]));
									continue;
								default:
									Log.WriteLine(true, false, "Warning: The special section {0} is not implemented in this build.", Section);
									continue;
							}
							//continue; //statement cannot be reached
						}

						int BeginValue = CfgFile[i].IndexOf("=");//regular sections
						if (BeginValue < 1) continue; //bad line
						string ParamName = CfgFile[i].Substring(0, BeginValue);
						string ParamValue = CfgFile[i].Substring(BeginValue + 1);
						//Log.WriteLine(true, false, "{0}.{1}={2}", Section, ParamName, ParamValue);

						//Log.WriteLine(true, false, Section);
						if (Section.StartsWith("FixableURL"))
						{
							//Log.WriteLine(true, false, "URL Fix rule: {0}/{1} = {2}",Section.Substring(11),ParamName,ParamValue);
							FixableUrlActions[Section.Substring(11)].Add(ParamName, ParamValue);
							continue;
						}

						if (Section.StartsWith("FixableType"))
						{
							FixableTypesActions[Section.Substring(12)].Add(ParamName, ParamValue);
							continue;
						}

						if (Section.StartsWith("ContentPatch:"))
						{
							if (!ContentPatches.Contains(Section.Substring(13))) ContentPatches.Add(Section.Substring(13));
							ContentPatchActions[Section.Substring(13)].Add(ParamName, ParamValue);
							continue;
						}

						if (Section.StartsWith("Edit:"))
						{
							if (RawEditSets.Count > 0)
								RawEditSets[LastRawEditSet].Add(CfgFile[i]);
							continue;
						}

						switch (Section)
						{
							case "Server":
								switch (ParamName)
								{
									case "Port":
										Port = Convert.ToInt32(ParamValue);
										break;
									case "OutputEncoding":
										if (ParamValue == "Windows" || ParamValue == "Win" || ParamValue == "ANSI")
										{
											//OutputEncoding = Encoding.Default; //.NET 4.0
											OutputEncoding = CodePagesEncodingProvider.Instance.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.ANSICodePage);
											continue;
										}
										else if (ParamValue == "DOS" || ParamValue == "OEM")
										{
											OutputEncoding = CodePagesEncodingProvider.Instance.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.OEMCodePage);
											continue;
										}
										else if (ParamValue == "Mac" || ParamValue == "Apple")
										{
											OutputEncoding = CodePagesEncodingProvider.Instance.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.MacCodePage);
											continue;
										}
										else if (ParamValue == "EBCDIC" || ParamValue == "IBM")
										{
											OutputEncoding = CodePagesEncodingProvider.Instance.GetEncoding(System.Globalization.CultureInfo.CurrentCulture.TextInfo.EBCDICCodePage);
											continue;
										}
										else if (ParamValue == "0" || ParamValue == "AsIs")
										{
											OutputEncoding = null;
											continue;
										}
										else
										{
											try
											{
												//OutputEncoding = Encoding.GetEncoding(ParamValue); 
												OutputEncoding = CodePagesEncodingProvider.Instance.GetEncoding(ParamValue);
												if (OutputEncoding == null)
													try { OutputEncoding = CodePagesEncodingProvider.Instance.GetEncoding(int.Parse(ParamValue)); } catch { }

												if (OutputEncoding == null && ParamValue.ToLower().StartsWith("utf"))
												{
													switch (ParamValue.ToLower())
													{
														case "utf-7":
															OutputEncoding = Encoding.UTF7;
															break;
														case "utf-8":
															OutputEncoding = Encoding.UTF8;
															break;
														case "utf-16":
														case "utf-16le":
															OutputEncoding = Encoding.Unicode;
															break;
														case "utf-16be":
															OutputEncoding = Encoding.BigEndianUnicode;
															break;
														case "utf-32":
														case "utf-32le":
															OutputEncoding = Encoding.UTF32;
															break;
													}
												}

												if (OutputEncoding == null)
												{ Log.WriteLine(true, false, "Warning: Unknown codepage {0}, using AsIs. See MSDN 'Encoding.GetEncodings Method' article for list of valid encodings.", ParamValue); };
											}
											catch (ArgumentException) { Log.WriteLine(true, false, "Warning: Bad codepage {0}, using {1}. Get list of available encodings at http://{2}:{3}/!codepages/.", ParamValue, OutputEncoding.EncodingName, ConfigFile.DefaultHostName, Port); }
										}
										continue;
									case "Authenticate":
										Authenticate = ParamValue;
										continue;
									case "HideClientErrors":
										HideClientErrors = ToBoolean(ParamValue);
										continue;
									case "SearchInArchive":
										SearchInArchive = ToBoolean(ParamValue);
										continue;
									case "ShortenArchiveErrors":
										ShortenArchiveErrors = ToBoolean(ParamValue);
										continue;
									case "SecurityProtocols":
										try { System.Net.ServicePointManager.SecurityProtocol = (System.Net.SecurityProtocolType)(int.Parse(ParamValue)); }
										catch (NotSupportedException) { Log.WriteLine(true, false, "Warning: Bad TLS version {1} ({0}), using {2} ({2:D}).", ParamValue, (System.Net.SecurityProtocolType)(int.Parse(ParamValue)), System.Net.ServicePointManager.SecurityProtocol); };
										continue;
									case "UserAgent":
										UserAgent = ParamValue;
										continue;
									case "DefaultHostName":
										DefaultHostName = ParamValue.Replace("%HostName%", Environment.MachineName);
										bool ValidHostName = (Environment.MachineName.ToLower() == DefaultHostName.ToLower());
										if (!ValidHostName) foreach (System.Net.IPAddress LocIP in Program.GetLocalIPAddresses())
											{ if (LocIP.ToString() == DefaultHostName) ValidHostName = true; }
										if (!ValidHostName)
										{ try { if (System.Net.Dns.GetHostEntry(DefaultHostName).AddressList.Count() > 0) ValidHostName = true; } catch { } }
										if (!ValidHostName) Log.WriteLine(true, false, "Warning: DefaultHostName setting is not applicable to this computer!");
										continue;
									case "ValidateCertificates":
										ValidateCertificates = ToBoolean(ParamValue);
										continue;
									case "TemporaryDirectory":
										if (ParamValue.ToUpper() == "%TEMP%" || ParamValue == "$TEMP" || ParamValue == "$TMPDIR") TemporaryDirectory = Path.GetTempPath();
										else TemporaryDirectory = ParamValue;
										continue;
									case "LogFile":
										if(OverrideLogFile != null && OverrideLogFile == "")
											LogAgent.OpenLogFile(GetLogFilePath(ParamValue), false);
										continue;
									case "AppendLogFile":
										if (OverrideLogFile != null && OverrideLogFile == "")
											LogAgent.OpenLogFile(GetLogFilePath(ParamValue), true);
										continue;
									case "AllowConfigFileDisplay":
										AllowConfigFileDisplay = ToBoolean(ParamValue);
										continue;
									case "DisplayStatusPage":
										DisplayStatusPage = ParamValue;
										continue;
									default:
										Log.WriteLine(true, false, "Warning: Unknown server option: " + ParamName);
										break;
								}
								break;
							case "Edit":
								if (RawEditSets.Count > 0)
									RawEditSets[LastRawEditSet].Add(CfgFile[i]);
								break;
							case "Translit":
								TranslitTable.Add(new KeyValuePair<string, string>(ParamName, ParamValue));
								break;
							default:
								Log.WriteLine(true, false, "Warning: Unknown section: " + Section);
								break;
						}

					}
					catch (Exception ex)
					{
						Log.WriteLine(true, false, "Error on line {1}: {0}", ex.Message, i);
#if DEBUG
						Log.WriteLine(true, false, "All next lines will be ignored. Invoking debugging.");
						throw;
#endif
					}
				}

				i++;
				foreach (List<string> RawEdit in RawEditSets)
				{
					EditRules.Add(new EditSet(RawEdit));
				}

				AddLegacyFixableURLs();
				AddLegacyFixableTypes();
				AddLegacyContentPatches();
			}
			catch(Exception ex) {
				#if DEBUG
				Log.WriteLine(true, false, "Error in configuration file: {0}. Line {1}. Go to debugger.", ex.ToString(), i);
				throw;
				#else
				Log.WriteLine(true, false, "Error in configuration file: {0}.\nAll next lines after {1} are ignored.", ex.Message, i);
				#endif
			}

			if (i < 1) Log.WriteLine(true, false, "Warning: curiously short file. Probably line endings are not valid for this OS.");
			Log.WriteLine(true, false, "{0} load complete.", ConfigFileName);
		}

		/// <summary>
		/// Convert string "true/false" or similar to bool true/false
		/// </summary>
		/// <param name="s">One of these strings: 1/0, y/n, yes/no, on/off, enable/disable, true/false</param>
		/// <returns>Boolean true/false</returns>
		/// <exception cref="InvalidCastException">Throws if the <paramref name="s"/> is not 1/0/y/n/yes/no/on/off/enable/disable/true/false</exception>
		public static bool ToBoolean(this string s)
		{
			//from https://stackoverflow.com/posts/21864625/revisions
			string[] trueStrings = { "1", "y", "yes", "on", "enable", "true" };
			string[] falseStrings = { "0", "n", "no", "off", "disable", "false" };


			if (trueStrings.Contains(s, StringComparer.OrdinalIgnoreCase))
				return true;
			if (falseStrings.Contains(s, StringComparer.OrdinalIgnoreCase))
				return false;

			throw new InvalidCastException("only the following are supported for converting strings to boolean: "
				+ string.Join(",", trueStrings)
				+ " and "
				+ string.Join(",", falseStrings));
		}

		/// <summary>
		/// Convert legacy FixableURL sections to new syntax (edit sets)
		/// </summary>
		private static void AddLegacyFixableURLs()
		{
			foreach(string FixUrl in FixableURLs)
			{
				List<string> RawES = new List<string>();
				RawES.Add("OnUrl="+FixUrl);

				foreach (KeyValuePair<string, string> FixUrlAct in FixableUrlActions[FixUrl])
				{
					switch (FixUrlAct.Key.ToLower())
					{
						case "validmask":
							RawES.Add("IgnoreUrl=" + FixUrlAct.Value);
							continue;
						case "redirect":
							RawES.Add("AddRedirect=" + FixUrlAct.Value);
							continue;
						case "internal":
							if(FixUrlAct.Value.ToLower() == "yes" && FixableUrlActions[FixUrl].ContainsKey("Redirect"))
							RawES.Add("AddInternalRedirect=" + FixableUrlActions[FixUrl]["Redirect"]);
							continue;
						default:
							Log.WriteLine(true, false, "Unknown legacy FixableURL option: {0}", FixUrlAct.Key);
							continue;
					}
				}
				EditRules.Add(new EditSet(RawES));
				//Log.WriteLine(true, false, new EditSet(RawES));
			}
		}

		/// <summary>
		/// Convert legacy FixableType sections to new syntax (edit sets)
		/// </summary>
		private static void AddLegacyFixableTypes()
		{
			foreach (string FixT in FixableTypes)
			{
				List<string> RawES = new List<string>();
				RawES.Add("OnContentType=" + FixT);
				RawES.Add("OnCode=2");

				foreach (KeyValuePair<string, string> FixTAct in FixableTypesActions[FixT])
				{
					switch (FixTAct.Key.ToLower())
					{
						case "ifurl":
							RawES.Add("OnUrl=" + FixTAct.Value);
							continue;
						case "noturl":
							RawES.Add("IgnoreUrl=" + FixTAct.Value);
							continue;
						case "redirect":
							RawES.Add("AddRedirect=" + FixTAct.Value);
							continue;
						default:
							Log.WriteLine(true, false, "Unknown legacy FixableType option: {0}", FixTAct.Key);
							continue;
					}
				}
				EditRules.Add(new EditSet(RawES));
				//Log.WriteLine(true, false, new EditSet(RawES));
			}
		}


		/// <summary>
		/// Convert legacy ContentPatch sections to new syntax (edit sets)
		/// </summary>
		private static void AddLegacyContentPatches()
		{
			foreach (string Patch in ContentPatches)
			{
				List<string> RawES = new List<string>();
				RawES.Add("AddFind=" + Patch);
				RawES.Add("OnCode=2");

				foreach (KeyValuePair<string, string> PatchAct in ContentPatchActions[Patch])
				{
					switch (PatchAct.Key.ToLower())
					{
						case "replace":
							RawES.Add("AddReplace=" + PatchAct.Value);
							continue;
						case "ifurl":
							RawES.Add("OnUrl=" + PatchAct.Value);
							continue;
						case "iftype":
							RawES.Add("OnContentType=" + PatchAct.Value);
							continue;
						default:
							Log.WriteLine(true, false, "Unknown legacy ContentPatch option: {0}", PatchAct.Key);
							continue;
					}
				}
				EditRules.Add(new EditSet(RawES));
				//Log.WriteLine(true, false, new EditSet(RawES));
			}
		}
	}
}
