﻿using System;
using System.ComponentModel;
using System.Deployment.Application;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TF2Items.Dialogs;
using TF2Items.Properties;


//using Windows7.DesktopIntegration;
//using Windows7.DesktopIntegration.WindowsForms;

namespace TF2Items
{
	public partial class MainForm : Form
	{
		private const int NumberOfItems = 4500;
		public const int NumberOfAttribs = 2700;
		private const int NumberOfSets = 200;
		private const int NumberOfSetItems = 6;
		private const int NumberOfSetAttribs = 2500;
		private static readonly Regex IsNumber = new Regex(@"^\d+$");

		private readonly string[] _attribAname = new string[NumberOfAttribs];
		private readonly string[] _attribClass = new string[NumberOfAttribs];
		private readonly double[] _attribMinvalue = new double[NumberOfAttribs];
		private readonly string[] _attribName = new string[NumberOfAttribs];

		private readonly string[] _baseitem = new string[NumberOfItems];
		private readonly string[] _craftClass = new string[NumberOfItems];
		private readonly string[] _imageInventory = new string[NumberOfItems];
		private readonly string[] _imageInventorySizeH = new string[NumberOfItems];
		private readonly string[] _imageInventorySizeW = new string[NumberOfItems];
		private readonly string[] _attachToHands = new string[NumberOfItems];
		private readonly string[] _itemClass = new string[NumberOfItems];
		private readonly string[] _itemName = new string[NumberOfItems];
		private readonly string[] _itemQuality = new string[NumberOfItems];
		private readonly string[] _itemSlot = new string[NumberOfItems];
		private readonly string[] _itemTypeName = new string[NumberOfItems];
		private readonly string[] _maxIlevel = new string[NumberOfItems];
		private readonly string[] _minIlevel = new string[NumberOfItems];
		private readonly string[] _modelPlayer = new string[NumberOfItems];
		private readonly string[] _name = new string[NumberOfItems];

		private readonly string[,] _itemAttribs = new string[NumberOfItems, NumberOfAttribs];
		private readonly double[,] _itemAttribsValue = new double[NumberOfItems, NumberOfAttribs];

		private readonly string[] _setName = new string[NumberOfSets];
		private readonly string[,] _setItems = new string[NumberOfSets, NumberOfSetItems];
		private readonly string[] _setBundle = new string[NumberOfSets];

		private readonly string[,] _setAttribsName = new string[NumberOfSets, NumberOfSetAttribs];
		private readonly double[,] _setAttribsValue = new double[NumberOfSets, NumberOfSetAttribs];

		private readonly bool[] _saved = new bool[14];
		private readonly int[,] _usedByClasses = new int[NumberOfItems, 9];

		private string _fileName;

		private bool _firstSetup;
		private string _lastTip;
		private double _percent;
		private int _saveNum = -1;
		private string _saveStr = "";
		private int _lastSel;
		private int _lastSet;
		private int _lastSetItem;
		private int _lastItem;

		//tf_english tab
		private string _engfileName;

		private const int NumberOfTips = 50;
		private readonly string[,] _engTips = new string[10, NumberOfTips];

		//Ctx tab
		private string _ctxFileName;

		public MainForm()
		{
			InitializeComponent();
		}
		private void Button1Click(object sender, EventArgs e)
		{
			filediagOpen.Title = Resources.Form1_button1_Click_Select_items_game_txt_or_team_fortress_2_content_gcf;

			filediagOpen.Filter =
				Resources.
					Form1_button1_Click_items_game_txt_items_game_txt_Team_Fortress_2_content_team_fortress_2_content_gcf;
			filediagOpen.RestoreDirectory = false;
			DialogResult result = filediagOpen.ShowDialog();
			if (result != DialogResult.OK) return;
			if (filediagOpen.FileName.Contains(".gcf"))
			{
				var extract = new Process
									{
										StartInfo =
											{
												FileName = "HLExtract.exe",
												Arguments = "-c -v -p \"" + filediagOpen.FileName +
															"\" -e \"root\\tf\\scripts\\items\\items_game.txt\""
											}
									};
				if (!File.Exists("HLExtract.exe"))
				{
					MessageBox.Show(Resources.Form1_button1_Click_HLExtract_exe_is_missing_from_the_program_folder_,
									Resources.Form1_button1_Click_Who_send_all_these_babies_to_fight__,
									MessageBoxButtons.OK,
									MessageBoxIcon.Error);
					return;
				}
				if (!File.Exists("HLLib.dll"))
				{
					MessageBox.Show(Resources.Form1_button1_Click_HLLib_dll_is_missing_from_the_program_folder_,
									Resources.Form1_button1_Click_Who_send_all_these_babies_to_fight__,
									MessageBoxButtons.OK,
									MessageBoxIcon.Error);
					return;
				}
				try
				{
					//extract.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					extract.Start();
					/* System.IO.WaitForChangedResult resultt;
					string directory = fileName.Replace("items_game.txt", "");
					System.IO.FileSystemWatcher watcher = new System.IO.FileSystemWatcher(directory, "items_game.txt");
					resultt = watcher.WaitForChanged(System.IO.WatcherChangeTypes.All);*/
				}
				catch
				{
					MessageBox.Show(Resources.Form1_button1_Click_Something_went_wrong_when_extracting___,
									Resources.Form1_button1_Click_Who_send_all_these_babies_to_fight__,
									MessageBoxButtons.OK,
									MessageBoxIcon.Error);
					return;
				}
				MessageBox.Show(
					Resources.Form1_button1_Click_Extracted_successfully_to_ + Environment.NewLine +
					filediagOpen.FileName.Replace("team fortress 2 content.gcf", "items_game.txt") +
					Resources.Form1_button1_Click_,
					Resources.Form1_button1_Click_Listen_up_,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				_fileName = filediagOpen.FileName.Replace("team fortress 2 content.gcf", "items_game.txt");
				if (!extract.HasExited)
				{
					extract.Kill();
					extract.Close();
				}
			}
			else _fileName = filediagOpen.FileName;
			_firstSetup = true;
			comboName.SelectedIndex = -1;
			comboName.Text = Resources.Form1_button1_Click_Select_an_item;
			for (int jj = 0; jj < NumberOfItems; jj++) for (int kk = 0; kk < 9; kk++) _usedByClasses[jj, kk] = 0;
			ReadFile();
		}

		public void ReadFile()
		{
			comboName.Items.Clear();
			grid_attribs.Rows.Clear();
			list_all_attribs.Items.Clear();
			list_available_classes.Items.Clear();
			list_used_by_classes.Items.Clear();
			txt_attach_to_hands.Clear();
			txt_baseitem.Clear();
			txt_craft_class.Clear();
			txt_image_inventory.Clear();
			txt_image_inventory_size_h.Clear();
			txt_image_inventory_size_w.Clear();
			txt_item_class.Clear();
			txt_item_name.Clear();
			txt_item_quality.Clear();
			txt_item_slot.Clear();
			txt_item_type_name.Clear();
			txt_max_ilevel.Clear();
			txt_min_ilevel.Clear();
			txt_model_player.Clear();
			comboSets.Items.Clear();
			listSetItems.Items.Clear();
			using (var sReader = new StreamReader(_fileName))
			{
				string line;
				string current = "";
				int i = 0;
				var file = new StreamReader(_fileName);
				_percent = file.BaseStream.Length / (double)100;
				bool usedby = false;
				bool inAttribs = false;
				int itemAtr = -1;
				string lastline = "";
				int level = 0;
				int aN = 0;
				bool inSets = false;
				int iSet = 0;
				int iSetItems = 0;
				bool inSetItems = false;
				bool inSetAttribs = false;
				int iSetAttribs = 0;
				bool inRecipes = false;
				while ((line = file.ReadLine()) != null)
				{
					progressReading.Value = (int)file.BaseStream.Position / (int)_percent > 100
												? 100
												: (int)file.BaseStream.Position / (int)_percent;
					// if (Osinfo.MajorVersion.ToString() + "." + Osinfo.MinorVersion.ToString() == "6.1") progressReading.SetTaskbarProgress(); //Only show progress bar on the taskbar if using Windows 7
					if (line.Contains("{")) level++;
					if (line.Contains("}")) level--;
					if (inRecipes) continue;
					if (line.Contains("\"attributes\"") && level == 1)
					{
						inAttribs = true;
						i = 0;
					}
					if (!inAttribs) //We're reading items
					{
						if (line.Contains("\"name\"") && !line.Contains("\t\t\t\t\t\t\"name\"") && !line.Contains("\"type\"")) //Parsing new item
						{
							string tmp = Regex.Replace(line.Replace("\"name\"", "").Replace("\t", ""), "(?<comment>//.*)", "");

							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							if (tmp.Substring(0, 1) == " ") tmp = tmp.Substring(1); //Removes leading space in Polycount class bundles
							comboName.Items.Insert(i, tmp + "\r\n");
							_name[i] = tmp;
							current = tmp;
							i++;
							aN = 0;
							//if (current == "afafassfag43") continue;
						}
						if (line.Contains("\"attributes\""))
						{
							itemAtr++;
							continue;
						}
						if (line.Contains("{") && itemAtr > 0) itemAtr++;
						if (line.Contains("}") && itemAtr > 0) itemAtr--;
						if (line.Contains("\"attribute_class\"") && itemAtr > 0) continue;
						if (line.Contains("}") && lastline.Contains("}")) itemAtr = 0;
						if (line.Contains("\"value\"") && itemAtr > 0)
						{
							string sumthin = Regex.Replace(line.Replace("\"", "").Replace("  ", "").Replace("value", "").Replace("\t", ""), "(?<comment>//.*)", "");
							double sum = Converter.ToDouble(sumthin);
							_itemAttribsValue[i - 1, aN] = sum;
							aN++;
						}
						if (line.Contains("\"force_gc_to_generate\"") && itemAtr > 0) continue;
						if (line.Contains("\"use_custom_logic\"") && itemAtr > 0) continue;
						if (line.Contains("\"") && itemAtr > 0 && !line.Contains("\"value\"")) _itemAttribs[i - 1, aN] = line.Replace("\"", "").Replace("  ", "").Replace("\t", "");

						#region Assign arrays

						if (line.Contains("\"item_class\""))
						{
							string tmp = line.Replace("\"item_class\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_itemClass[i - 1] = tmp;
						}
						if (line.Contains("\"craft_class\""))
						{
							string tmp = line.Replace("\"craft_class\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_craftClass[i - 1] = tmp;
						}
						if (line.Contains("\"item_type_name\""))
						{
							string tmp = line.Replace("\"item_type_name\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_itemTypeName[i - 1] = tmp;
						}
						if (line.Contains("\"item_name\""))
						{
							string tmp = line.Replace("\"item_name\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_itemName[i - 1] = tmp;
						}
						if (line.Contains("\"item_slot\""))
						{
							string tmp = line.Replace("\"item_slot\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_itemSlot[i - 1] = tmp;
						}
						if (line.Contains("\"item_quality\""))
						{
							string tmp = line.Replace("\"item_quality\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_itemQuality[i - 1] = tmp;
						}
						if (line.Contains("\"baseitem\""))
						{
							string tmp = line.Replace("\"baseitem\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_baseitem[i - 1] = tmp.Replace(" ", "");
						}
						if (line.Contains("\"min_ilevel\""))
						{
							string tmp = line.Replace("\"min_ilevel\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_minIlevel[i - 1] = tmp.Replace(" ", "");
						}
						if (line.Contains("\"max_ilevel\""))
						{
							string tmp = line.Replace("\"max_ilevel\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_maxIlevel[i - 1] = tmp.Replace(" ", "");
						}
						if (line.Contains("\"image_inventory\""))
						{
							string tmp = line.Replace("\"image_inventory\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_imageInventory[i - 1] = tmp;
						}
						if (line.Contains("\"image_inventory_size_w\""))
						{
							string tmp = line.Replace("\"image_inventory_size_w\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_imageInventorySizeW[i - 1] = tmp.Replace(" ", "");
						}
						if (line.Contains("\"image_inventory_size_h\""))
						{
							string tmp = line.Replace("\"image_inventory_size_h\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_imageInventorySizeH[i - 1] = tmp.Replace(" ", "");
						}
						if (line.Contains("\"model_player\""))
						{
							string tmp = line.Replace("\"model_player\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_modelPlayer[i - 1] = tmp;
						}
						if (line.Contains("\"attach_to_hands\""))
						{
							string tmp = line.Replace("\"attach_to_hands\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_attachToHands[i - 1] = tmp.Replace(" ", "");
						}

						#endregion

						if (line.Contains("\"used_by_classes\"")) usedby = true;
						if (line.Contains("}") && usedby) usedby = false;
						if (line.Contains("\"1\"") && usedby)
						{
							string tmp = line.Replace("\"1\"", "").Replace("\t", "");
							int res;
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							tmp = tmp.Replace(" ", "");

							#region Classes switch

							switch (tmp)
							{
								case "scout":
									res = 0;
									break;
								case "soldier":
									res = 1;
									break;
								case "pyro":
									res = 2;
									break;
								case "demoman":
									res = 3;
									break;
								case "heavy":
									res = 4;
									break;
								case "engineer":
									res = 5;
									break;
								case "medic":
									res = 6;
									break;
								case "sniper":
									res = 7;
									break;
								case "spy":
									res = 8;
									break;
								default:
									//MessageBox.Show("Invalid class in " + current + "/used_by_classes! Line = " + line);
									res = 0;
									break;
							}

							#endregion

							_usedByClasses[i - 1, res] = 1;
						}
					}
					else if (!inSets) //We're reading global attributes list
					{
						if (line.Contains("\t\"recipes\""))
						{
							inRecipes = true;
							continue;
						}
						if (line.Contains("\"item_sets\""))
						{
							inSets = true;
							continue;
						}
						if (line.Contains("\"name\""))
						{
							string tmp = line.Replace("\"name\"", "").Replace("\t", "");
							tmp = tmp.Replace("\"", "");
							tmp = tmp.Replace("  ", "");
							_attribName[i] = tmp;
							list_all_attribs.Items.Add(tmp);
							i++;
						}
						else if (line.Contains("\"attribute_class\""))
						{
							_attribClass[i - 1] =
								line.Replace("\"attribute_class\"", "").Replace("\"", "").Replace("\t", "");
						}
						else if (line.Contains("\"attribute_name\""))
						{
							_attribAname[i - 1] =
								line.Replace("\"attribute_name\"", "").Replace("\"", "").Replace("\t", "");
						}
						else if (line.Contains("\"attribute_name\""))
						{
							_attribMinvalue[i - 1] =
								Converter.ToDouble(
									line.Replace("\"min_value\"", "").Replace("\"", "").Replace("\t", ""));
						}
					}
					else //We're reading sets list
					{
						if (level == 1 && line.Contains("}"))
						{
							inSets = false;
							continue;
						}
						if (!inSetAttribs)
						{
							if (level == 2 && line.Contains("\""))
							{
								comboSets.Items.Add(line.Replace("\"", "").Replace("\t", ""));
								continue;
							}
							if (line.Contains("\"name\""))
							{
								_setName[iSet] = line.Replace("\"name\"", "").Replace("\"", "").Replace("\t", "");
								iSet++;
								iSetItems = 0;
								iSetAttribs = 0;
								continue;
							}
							if (line.Contains("\"items\""))
							{
								inSetItems = true;
								continue;
							}
							if (inSetItems && line.Contains("\"1\""))
							{
								_setItems[iSet - 1, iSetItems] = line.Replace("\"1\"", "").Replace("\t", "").Replace("\"", "");
								iSetItems++;
								continue;
							}
							if (inSetItems && line.Contains("}"))
							{
								inSetItems = false;
								continue;
							}
							if (line.Contains("\"store_bundle\""))
							{
								_setBundle[iSet - 1] = line.Replace("\"store_bundle\"", "").Replace("\t", "").Replace(" \"", "").Replace("\"", "");
								continue;
							}
							if (line.Contains("\"attributes\""))
							{
								inSetAttribs = true;
								continue;
							}
						}
						else
						{
							if (line.Contains("\"attribute_class\"")) continue;
							if (line.Contains("\"") && level == 4 && !line.Contains("\"value\""))
							{
								_setAttribsName[iSet - 1, iSetAttribs] = line.Replace("\t", "").Replace("\"", "");
								continue;
							}
							if (line.Contains("\"value\""))
							{
								_setAttribsValue[iSet - 1, iSetAttribs] = Convert.ToDouble(line.Replace("\t", "").Replace("value", "").Replace("\"", ""));
								iSetAttribs++;
							}
							if (line.Contains("}") && lastline.Contains("}"))
							{
								inSetAttribs = false;
							}
						}
					}


					lastline = line;
				}
				file.Close();
				sReader.Close();
				_attribAname[113] = "mod mini-crit airborne";
				//Somehow the mini-crit airborne doesn't have attribute_name, here's a workaround
				comboName.SelectedIndex = comboName.SelectedIndex;
				progressReading.Value = 100;
				foreach (object itm in comboName.Items)
				{
					comboAddSetItem.Items.Add(itm);
				}
				//if(Osinfo.MajorVersion.ToString() + "." + Osinfo.MinorVersion.ToString() == "6.1") Windows7.DesktopIntegration.Windows7Taskbar.SetProgressState(this.Handle, Windows7Taskbar.ThumbnailProgressState.NoProgress);
			}
			_firstSetup = false;
			comboName.SelectedIndex = 0;
			comboAddSetItem.SelectedIndex = 0;
			comboSets.SelectedIndex = 0;
		}

		/// <summary>Returns a TF2 class name based on the classid</summary>
		/// <param name="classid">The class id, starting from 0</param>
		/// <returns>The class name, lowercase or "Invalid classid!" on error</returns>
		public static string GetClassName(int classid)
		{
			switch (classid)
			{
				case 0:
					return "scout";
				case 1:
					return "soldier";
				case 2:
					return "pyro";
				case 3:
					return "demoman";
				case 4:
					return "heavy";
				case 5:
					return "engineer";
				case 6:
					return "medic";
				case 7:
					return "sniper";
				case 8:
					return "spy";
				default:
					return "Invalid classid!";
			}
		}

		/// <summary>Returns a TF2 class id based on the name</summary>
		/// <param name="classname">The class name, lowercase</param>
		/// <returns>The class id, starting from 0 or -1 on error</returns>
		public static int GetClassId(string classname)
		{
			switch (classname)
			{
				case "scout":
					return 0;
				case "soldier":
					return 1;
				case "pyro":
					return 2;
				case "demoman":
					return 3;
				case "heavy":
					return 4;
				case "engineer":
					return 5;
				case "medic":
					return 6;
				case "sniper":
					return 7;
				case "spy":
					return 8;
				default:
					return -1;
			}
		}

		/// <summary>
		/// Returns a bool indicating if the item has attributes
		/// </summary>
		/// <param name="itemid">The id of the item</param>
		public bool DoesItemHaveAttribs(int itemid)
		{
			if (itemid < 0 || itemid >= NumberOfItems) return false;
			for (int i = 0; i < NumberOfAttribs; i++) if (_itemAttribs[itemid, i] != null) return true;
			return false;
		}

		public static bool IsNumeric(string theValue)
		{
			Match m = IsNumber.Match(theValue);
			return m.Success;
		}

		private string GetAttribClass(string attribname)
		{
			for (int k = 0; k < NumberOfAttribs; k++) if (_attribName[k] == attribname) return _attribClass[k];
			return "";
		}

		/*
				private string GetAttribAname(string attribname)
				{
					for (int k = 0; k < number_of_attribs; k++) if (attrib_name[k] == attribname) return attrib_aname[k];
					return "";
				}
		*/

		public string ReturnSettingVal(int item, int sId)
		{
			if (item < 0 || item >= NumberOfItems || sId < 0 || sId > 13) return null;
			switch (sId)
			{
				case 0:
					return _itemClass[item];
				case 1:
					return _craftClass[item];
				case 2:
					return _itemTypeName[item];
				case 3:
					return _itemName[item];
				case 4:
					return _itemSlot[item];
				case 5:
					return _itemQuality[item];
				case 6:
					return _baseitem[item];
				case 7:
					return _minIlevel[item];
				case 8:
					return _maxIlevel[item];
				case 9:
					return _imageInventory[item];
				case 10:
					return _imageInventorySizeW[item];
				case 11:
					return _imageInventorySizeH[item];
				case 12:
					return _modelPlayer[item];
				case 13:
					return _attachToHands[item];
			}
			return "";
		}

		public string ReturnSettingStr(int sId)
		{
			if (sId < 0 || sId > 13) return null;
			switch (sId)
			{
				case 0:
					return "item_class";
				case 1:
					return "craft_class";
				case 2:
					return "item_type_name";
				case 3:
					return "item_name";
				case 4:
					return "item_slot";
				case 5:
					return "item_quality";
				case 6:
					return "baseitem";
				case 7:
					return "min_ilevel";
				case 8:
					return "max_ilevel";
				case 9:
					return "image_inventory";
				case 10:
					return "image_inventory_size_w";
				case 11:
					return "image_inventory_size_h";
				case 12:
					return "model_player";
				case 13:
					return "attach_to_hands";
			}

			return "";
		}

		public int GetAttribId(string attribName)
		{
			for (int i = 0; i < NumberOfAttribs; i++) if (_attribName[i] == attribName) return i;
			return -1;
		}

		private void ComboNameSelectedIndexChanged(object sender, EventArgs e) //When user selects an item
		{
			if (comboName.SelectedIndex == -1) return;
			_firstSetup = true;

			txt_item_class.Text = _itemClass[comboName.SelectedIndex];
			txt_craft_class.Text = _craftClass[comboName.SelectedIndex];
			txt_item_type_name.Text = _itemTypeName[comboName.SelectedIndex];
			txt_item_name.Text = _itemName[comboName.SelectedIndex];
			txt_item_slot.Text = _itemSlot[comboName.SelectedIndex];
			txt_item_quality.Text = _itemQuality[comboName.SelectedIndex];
			txt_baseitem.Text = _baseitem[comboName.SelectedIndex];
			txt_min_ilevel.Text = _minIlevel[comboName.SelectedIndex];
			txt_max_ilevel.Text = _maxIlevel[comboName.SelectedIndex];
			txt_image_inventory.Text = _imageInventory[comboName.SelectedIndex];
			txt_image_inventory_size_w.Text = _imageInventorySizeW[comboName.SelectedIndex];
			txt_image_inventory_size_h.Text = _imageInventorySizeH[comboName.SelectedIndex];
			txt_model_player.Text = _modelPlayer[comboName.SelectedIndex];
			txt_attach_to_hands.Text = _attachToHands[comboName.SelectedIndex];

			txt_item_class.Enabled = comboName.SelectedIndex > 30;
			txt_craft_class.Enabled = comboName.SelectedIndex > 30;
			txt_item_type_name.Enabled = comboName.SelectedIndex > 30;
			txt_item_name.Enabled = comboName.SelectedIndex > 30;
			txt_item_slot.Enabled = comboName.SelectedIndex > 30;
			txt_item_quality.Enabled = comboName.SelectedIndex > 30;
			txt_baseitem.Enabled = comboName.SelectedIndex > 30;
			txt_min_ilevel.Enabled = comboName.SelectedIndex > 30;
			txt_max_ilevel.Enabled = comboName.SelectedIndex > 30;
			txt_image_inventory.Enabled = comboName.SelectedIndex > 30;
			txt_image_inventory_size_w.Enabled = comboName.SelectedIndex > 30;
			txt_image_inventory_size_h.Enabled = comboName.SelectedIndex > 30;
			txt_model_player.Enabled = comboName.SelectedIndex > 30;
			txt_attach_to_hands.Enabled = comboName.SelectedIndex > 30;
			list_all_attribs.Enabled = true;
			list_used_by_classes.Enabled = comboName.SelectedIndex > 30;
			list_available_classes.Enabled = comboName.SelectedIndex > 30;
			move_left_btn.Enabled = comboName.SelectedIndex > 30;
			move_right_btn.Enabled = comboName.SelectedIndex > 30;
			grid_attribs.Enabled = comboName.SelectedIndex > 30;
			searchBox.Enabled = true;
			comboSets.Enabled = true;
			textSetName.Enabled = true;
			textStoreBundle.Enabled = true;
			comboAddSetItem.Enabled = true;
			listSetItems.Enabled = true;
			gridSet.Enabled = true;
			btnCopy.Enabled = true;
			btnPaste.Enabled = true;
			btnAddSet.Enabled = true;
			btnDelSet.Enabled = true;
			button2.Enabled = true;
			button3.Enabled = true;

			list_used_by_classes.Items.Clear();
			list_available_classes.Items.Clear();

			for (int i = 0; i < 9; i++)
			{
				if (_usedByClasses[comboName.SelectedIndex, i] == 1) list_used_by_classes.Items.Add(GetClassName(i));
				else list_available_classes.Items.Add(GetClassName(i));
			}
			int c = 0;
			grid_attribs.Rows.Clear();
			for (int j = 0; j < NumberOfAttribs; j++)
			{
				if (_itemAttribs[comboName.SelectedIndex, j] != null && _itemAttribs[comboName.SelectedIndex, j] != "")
				{
					int n = grid_attribs.Rows.Add();
					grid_attribs.Rows[n].Cells[0].Value = _itemAttribs[comboName.SelectedIndex, j];
					grid_attribs.Rows[n].Cells[1].Value = GetAttribClass(_itemAttribs[comboName.SelectedIndex, j]);
					grid_attribs.Rows[n].Cells[2].Value = _itemAttribsValue[comboName.SelectedIndex, j].ToString().Replace(",", ".");
					if (!_itemAttribs[comboName.SelectedIndex, j].Contains("custom employee number")) c++;
				}
			}
			//list_all_attribs.Enabled = c != 0;
			_firstSetup = false;
		}

		private void MoveLeftBtnClick(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			if (list_available_classes.SelectedItem == null) return;
			list_used_by_classes.Items.Add(list_available_classes.SelectedItem);
			list_available_classes.Items.Remove(list_available_classes.SelectedItem);
			for (int i = 0; i < 9; i++)
			{
				if (list_used_by_classes.Items.Contains(GetClassName(i))) _usedByClasses[comboName.SelectedIndex, i] = 1;
				else _usedByClasses[comboName.SelectedIndex, i] = 0;
			}
		}

		private void MoveRightBtnClick(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			if (list_used_by_classes.SelectedItem == null) return;
			list_available_classes.Items.Add(list_used_by_classes.SelectedItem);
			list_used_by_classes.Items.Remove(list_used_by_classes.SelectedItem);
			for (int i = 0; i < 9; i++)
			{
				if (list_used_by_classes.Items.Contains(GetClassName(i))) _usedByClasses[comboName.SelectedIndex, i] = 1;
				else _usedByClasses[comboName.SelectedIndex, i] = 0;
			}
		}
		private void ListAllAttribsDoubleClick(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex <= 30) return;
			if (radioItem.Checked)
			{
				if (comboName.SelectedIndex == -1) return;
				int n = grid_attribs.Rows.Add();
				grid_attribs.Rows[n].Cells[0].Value = list_all_attribs.SelectedItem.ToString();
				grid_attribs.Rows[n].Cells[1].Value = GetAttribClass(list_all_attribs.SelectedItem.ToString());
				grid_attribs.Rows[n].Cells[2].Value = "0";
				_itemAttribs[comboName.SelectedIndex, n] = list_all_attribs.SelectedItem.ToString();
			}
			else if (radioSet.Checked)
			{
				if (comboSets.SelectedIndex == -1) return;
				int n = gridSet.Rows.Add();
				gridSet.Rows[n].Cells[0].Value = list_all_attribs.SelectedItem.ToString();
				gridSet.Rows[n].Cells[1].Value = GetAttribClass(list_all_attribs.SelectedItem.ToString());
				gridSet.Rows[n].Cells[2].Value = "0";
				_setAttribsName[comboSets.SelectedIndex, n] = list_all_attribs.SelectedItem.ToString();
			}
		}

		private void GridAttribsCellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			if (e == null) return;
			int i = 0;
			if (e.ColumnIndex == 0 || _firstSetup) return;
			foreach (DataGridViewRow roo in grid_attribs.Rows)
			{
				if (roo.Cells[2].Value == null) continue;
				_itemAttribsValue[comboName.SelectedIndex, i] = Converter.ToDouble(roo.Cells[2].Value.ToString());
				_itemAttribs[comboName.SelectedIndex, i] = roo.Cells[0].Value.ToString();
				i++;
			}
		}

		public void SaveFile()
		{
			var newFile = new StringBuilder();
			if (!File.Exists(_fileName))
			{
				FileStream rofl = File.Create(_fileName);
				rofl.Close();
			}
			string[] file = File.ReadAllLines(_fileName);

			int i = 0;
			string temp;
			int itemAtr = 0;
			string lastline = "";
			string current = "";
			bool don = false;
			bool usedBy = false;
			int level = 0;
			bool inTools = false;
			bool inSets = false;
			bool inAttribs = false;
			bool inSetItems = false;
			var wasInSet = new string[NumberOfSets];
			int inSetItem = 0;
			bool ended = false;
			bool inVisuals = false;
			for (int ii = 0; ii < 14; ii++) _saved[ii] = false;
			foreach (string line in file)
			{
				progressReading.Value = newFile.Length / (int)_percent > 100 ? (100) : (newFile.Length / (int)_percent);
				//if (Osinfo.MajorVersion.ToString() + "." + Osinfo.MinorVersion.ToString() == "6.1") progressReading.SetTaskbarProgress(); //Only show progress bar on the taskbar if using Windows 7

				temp = line;
				if (line.Contains("\"items_game\"") && !lastline.Contains("//"))
				{
					temp = "//items_game.txt generated using " + Text + " (Created by bogeyman_EST)\r\n" + line;
				}


				if (line.Contains("{")) level++;
				if (line.Contains("}")) level--;
				if (ended) goto end;
				if (inVisuals && line.Contains("}") && level == 3) inVisuals = false;
				if (line.Contains("\"visuals_blu\"") || line.Contains("\"visuals_red\"") || line.Contains("\"visuals\"")) inVisuals = true;
				if (inVisuals) goto end;
				if (line.Contains("\"attributes\"") && level == 1 && !inAttribs) //The list of attributes
				{
					inAttribs = true;
					goto end;
				}
				if (line.Contains("}") && level == 1 && inAttribs)
				{
					inAttribs = false;
					goto end;
				}
				if (line.Contains("\"recipes\""))
				{
					inSets = false;
					ended = true;
					temp = "\t}\r\n" + temp;
					goto end;
				}

				if (inAttribs) goto end;
				if (inSets) continue;
				if (line.Contains("\"item_sets\""))
				{
					inSets = true;
					temp += "\r\n\t{";
				}
				if (line.Contains("\"item_set\""))
				{
					string ss = line.Replace("\t", "").Replace("\"item_set\"", "").Replace("\"", "").Replace("\r\n", "");
					for (int k = 0; k < comboSets.Items.Count; k++)
					{
						bool brk = false;
						if (comboSets.Items[k].ToString() != ss) continue;
						for (int j = 0; j < NumberOfSetItems; j++)
						{
							if (_setItems[k, j] != current) continue;
							break;
						}
						wasInSet[inSetItem] = ss;
						inSetItem++;
						goto end;
					}
					continue;
				}
				if (inSets)
				{
					int iSet = 0;
					foreach (object set in comboSets.Items)
					{
						temp += "\r\n\t\t\"" + set + "\"\r\n\t\t{\r\n";
						if (_setName[iSet] != null && _setName[iSet] != "") temp += "\t\t\t\"name\"\t\"" + _setName[iSet] + "\"\r\n";
						temp += "\t\t\t\"items\"\r\n\t\t\t{";
						for (int j = 0; j < NumberOfSetItems; j++)
						{
							if (_setItems[iSet, j] == null || _setItems[iSet, j] == "") continue;
							temp += "\r\n\t\t\t\t\"" + _setItems[iSet, j].TrimEnd(new char[] {' '}) + "\"\t\"1\"";
						}
						temp += "\r\n\t\t\t}\r\n";
						if (DoesSetHaveAttribs(iSet))
						{
							temp += "\t\t\t\"attributes\"\r\n\t\t\t{";
							for (int j = 0; j < NumberOfSetAttribs; j++)
							{
								if (_setAttribsName[iSet, j] == null || _setAttribsName[iSet, j] == "") continue;
								temp += "\r\n\t\t\t\t\"" + _setAttribsName[iSet, j] + "\"\r\n\t\t\t\t{\r\n";
								temp += "\t\t\t\t\t\"attribute_class\"\t\"" + GetAttribClass(_setAttribsName[iSet, j]) + "\"\r\n";
								temp += "\t\t\t\t\t\"value\"\t\"" + _setAttribsValue[iSet, j].ToString().Replace(",", ".") + "\"\r\n\t\t\t\t}";
							}
							temp += "\r\n\t\t\t}\r\n";
						}
						if (_setBundle[iSet] != null && _setBundle[iSet] != "") temp += "\t\t\t\"store_bundle\"\t\"" + _setBundle[iSet] + "\"\r\n";
						temp += "\t\t}";
						iSet++;
					}
					goto end;
				}

				if (line.Contains("\"tool\"") && !line.Contains("_class"))
				{
					inTools = true;
					goto end;
				}
				if (level == 4 && inTools) goto end;
				if (level == 3 && inTools)
				{
					inTools = false;
					goto end;
				}
				if (line.Contains("\"name\"") && level == 3) //Parsing new item
				{
					i++;
					current = line.Replace("name", "").Replace("\"", "").Replace("\t", "");
					don = false;
					for (int ii = 0; ii < 14; ii++) _saved[ii] = false;
					temp = i < NumberOfItems ? line.Replace(current, _name[i - 1]) : line;
					inSetItem = 0;
					for (int h = 0; h < wasInSet.Length; h++)
					{
						wasInSet[h] = null;
					}
					goto end;
				}

				#region Write from arrays

				if (line.Contains("\"item_class\""))
				{
					temp = line.Replace("\"item_class\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _itemClass[i - 1]);
					_saved[0] = true;
					goto end;
				}
				if (line.Contains("\"craft_class\""))
				{
					temp = line.Replace("\"craft_class\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _craftClass[i - 1]);
					_saved[1] = true;
					goto end;
				}
				if (line.Contains("\"item_type_name\""))
				{
					temp = line.Replace("\"item_type_name\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _itemTypeName[i - 1]);
					_saved[2] = true;
					goto end;
				}
				if (line.Contains("\"item_name\""))
				{
					temp = line.Replace("\"item_name\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _itemName[i - 1]);
					_saved[3] = true;
					goto end;
				}
				if (line.Contains("\"item_slot\""))
				{
					temp = line.Replace("\"item_slot\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _itemSlot[i - 1]);
					_saved[4] = true;
					goto end;
				}
				if (line.Contains("\"item_quality\""))
				{
					temp = line.Replace("\"item_quality\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _itemQuality[i - 1]);
					_saved[5] = true;
					goto end;
				}
				if (line.Contains("\"baseitem\""))
				{
					temp = line.Replace("\"baseitem\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "").Replace(" ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _baseitem[i - 1]);
					_saved[6] = true;
					goto end;
				}
				if (line.Contains("\"min_ilevel\""))
				{
					temp = line.Replace("\"min_ilevel\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _minIlevel[i - 1]);
					_saved[7] = true;
					goto end;
				}
				if (line.Contains("\"max_ilevel\""))
				{
					temp = line.Replace("\"max_ilevel\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _maxIlevel[i - 1]);
					_saved[8] = true;
					goto end;
				}
				if (line.Contains("\"image_inventory\""))
				{
					temp = line.Replace("\"image_inventory\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _imageInventory[i - 1]);
					_saved[9] = true;
					goto end;
				}
				if (line.Contains("\"image_inventory_size_w\""))
				{
					temp = line.Replace("\"image_inventory_size_w\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _imageInventorySizeW[i - 1]);
					_saved[10] = true;
					goto end;
				}
				if (line.Contains("\"image_inventory_size_h\""))
				{
					temp = line.Replace("\"image_inventory_size_h\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _imageInventorySizeH[i - 1]);
					_saved[11] = true;
					goto end;
				}
				if (line.Contains("\"model_player\""))
				{
					temp = line.Replace("\"model_player\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _modelPlayer[i - 1]);
					_saved[12] = true;
					goto end;
				}
				if (line.Contains("\"attach_to_hands\""))
				{
					temp = line.Replace("\"attach_to_hands\"", "");
					temp = temp.Replace("\"", "");
					temp = temp.Replace("  ", "");
					if (temp.Length == 0)
					{
						temp = line;
						goto end;
					}
					temp = line.Replace(temp, _attachToHands[i - 1]);
					_saved[13] = true;
					goto end;
				}

				#endregion
				if (current == "") goto end;
				//if (current.Contains("Upgradeable")) goto end;
				if (line.Contains("\"attributes\"") && !current.Contains("Employee Badge"))
				{
					itemAtr++;
					don = false;
					temp += "\r\n\t\t\t{";
					goto end;
				}
				if (line.Contains("{") && itemAtr > 0)
				{
					itemAtr++;
					continue;
				}
				if (line.Contains("}") && itemAtr > 0)
				{
					itemAtr--;
					continue;
				}
				if (line.Contains("}") && itemAtr > 1) continue;
				if (line.Contains("{") && itemAtr > 1) continue;
				if ((line.Contains("\"attribute_class\"") || line.Contains("\"value\"")) && itemAtr > 0) continue;
				if (line.Contains("}") && lastline.Contains("}")) itemAtr = 0;
				if (line.Contains("\"") && don && itemAtr > 0) continue;
				if (line.Contains("\"") && itemAtr > 0 && !don)
				{
					temp = "";
					for (int j = 0; j < NumberOfAttribs; j++)
					{
						if (i >= NumberOfItems - 1) break;
						if (_itemAttribs[i - 1, j] == "custom employee number")
						{
							//Badges have weird attributes, here's a dirty workaround...
							temp =
								"\t\t\t\t\"custom employee number\"\r\n\t\t\t\t{\r\n\t\t\t\t\t\"attribute_class\"\t\"set_employee_number\"\r\n\t\t\t\t\t\"force_gc_to_generate\"\t\"1\"\r\n\t\t\t\t\t\"use_custom_logic\"\t\"employee_number\"\r\n\t\t\t\t}\r\n";
							continue;
						}
						if (_itemAttribs[i - 1, j] == "set supply crate series")
						{
							//So do supply crates
							temp =
								"\t\t\t\t\"set supply crate series\"\r\n\t\t\t\t{\r\n\t\t\t\t\t\"attribute_class\"\t\"supply_crate_series\"\r\n\t\t\t\t\t\"value\"\t\"" + _itemAttribsValue[i - 1, j] + "\"\r\n\t\t\t\t\t\"force_gc_to_generate\"\t\"1\"\r\n\t\t\t\t}\r\n";
							continue;
						}
						if (current == "Paint Can")
						{
							if (j == 0)
							{
								//And paint cans
								temp =
									"\t\t\t\t\"set item tint RGB\"\r\n\t\t\t\t{\r\n\t\t\t\t\t\"attribute_class\"\t\"set_item_tint_rgb\"\r\n\t\t\t\t\t\"force_gc_to_generate\"\t\"1\"\r\n\t\t\t\t}\r\n";
							}
						}
						if (GetAttribClass(_itemAttribs[i - 1, j]) != null &&
							GetAttribClass(_itemAttribs[i - 1, j]) != "")
						{
							temp = temp + "\t\t\t\t\"" + _itemAttribs[i - 1, j] +
									 "\"\r\n\t\t\t\t{\r\n\t\t\t\t\t\"attribute_class\"\t" + "\"" +
									 GetAttribClass(_itemAttribs[i - 1, j]) + "\"\r\n\t\t\t\t\t" + "\"value\"\t" + "\"" +
									 _itemAttribsValue[i - 1, j].ToString().Replace(",", ".") + "\"\r\n\t\t\t\t}\r\n";
						}
						if (j + 1 == NumberOfAttribs)
						{
							temp += "\t\t\t}";
							itemAtr--;
							break;
						}
						if (GetAttribClass(_itemAttribs[i - 1, j + 1]) != null) continue;
						temp += "\t\t\t}";
						itemAtr--;
						break;
					}

					don = true;
				}
				if (line.Contains("\"used_by_classes\"") && !current.Contains("Upgradeable TF_"))
				{
					usedBy = true;
					goto end;
				}
				if (line.Contains("{") && usedBy)
				{
					for (int j = 0; j < 9; j++)
					{
						if (i >= NumberOfItems - 1) break;
						if (_usedByClasses[i - 1, j] == 1) temp += "\r\n\t\t\t\t\"" + GetClassName(j) + "\"\t\"1\"";
					}
					temp += "\r\n\t\t\t}";
					goto end;
				}
				if (line.Contains("\"1\"")) if (usedBy) if (!IsNumeric(line.Replace("\"", "").Replace("\t", "").Replace(" ", ""))) continue;
				if (line.Contains("}") && usedBy)
				{
					usedBy = false;
					continue;
				}
				if (line.Contains("}") && level == 2)
				{
					temp = "";
					if (!don && DoesItemHaveAttribs(i - 1) && !current.Contains("Employee Badge"))
					{
						temp = "\t\t\t\"attributes\"\r\n\t\t\t{\r\n";
						for (int j = 0; j < NumberOfAttribs; j++)
						{
							if (i >= NumberOfItems - 1) break;
							if (GetAttribClass(_itemAttribs[i - 1, j]) != null)
							{
								temp += "\t\t\t\t\"" + _itemAttribs[i - 1, j] +
										"\"\r\n\t\t\t\t{\r\n\t\t\t\t\t\"attribute_class\"\t" + "\"" +
										GetAttribClass(_itemAttribs[i - 1, j]) + "\"\r\n\t\t\t\t\t" + "\"value\"\t" + "\"" +
										_itemAttribsValue[i - 1, j].ToString().Replace(",", ".") + "\"\r\n\t\t\t\t}\r\n";
							}
							if (GetAttribClass(_itemAttribs[i - 1, j + 1]) != null) continue;
							temp += "\t\t\t}";
							itemAtr--;
							break;
						}
						don = true;
					}
					int count = 0;

					for (int k = 0; k < 14; k++)
					{
						if (_saved[k] || ReturnSettingVal(i - 1, k) == null || ReturnSettingVal(i - 1, k) == "") continue;
						temp += "\r\n\t\t\t\"" + ReturnSettingStr(k) + "\"\t\"" + ReturnSettingVal(i - 1, k) + "\"";
						_saved[k] = true;
						count++;
					}
					for (int k = 0; k < comboSets.Items.Count; k++)
					{
						bool brk = false;
						for (int j = 0; j < NumberOfSetItems; j++)
						{
							if (current != _setItems[k, j]) continue;
							for (int h = 0; h < wasInSet.Length; h++)
							{
								string itm = comboSets.Items[k].ToString();
								if (wasInSet[h] == itm)
								{
									brk = true;
									break;
								}
								if (h != wasInSet.Length - 1) continue;
								temp += (don ? "\r\n" : "") + "\t\t\t\"item_set\"\t\"" + itm + "\"\r\n";
								count = 0;
								break;
							}
							if (brk) break;
						}
						if (brk) break;
					}
					temp += count > 0 ? "\r\n\t\t}" : "\t\t}";
					goto end;
				}
				if (IsNumeric(line.Replace("\"", "").Replace("\t", "").Replace(" ", "")) && lastline.Contains("\t\t\t}") && !lastline.Contains("\"item_set\"") && lastline.Contains("\"attributes\"")) temp = "\t\t}\r\n" + line;
			end:
				newFile.Append(temp + "\r\n");
				lastline = temp;
			}
			try
			{
				var lol = new StreamWriter(_fileName);
				lol.Write(newFile.ToString());
				lol.Close();
			}
			catch
			{
				MessageBox.Show(Resources.Form1_SaveFile_Something_went_wrong_while_saving_,
								Resources.Form1_SaveFile_Who_send_all_these_babies_to_fight__,
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
			}
			progressReading.Value = 100;
		}

		private bool DoesSetHaveAttribs(int p)
		{
			for (int i = 0; i < NumberOfSetAttribs; i++)
			{
				if (_setAttribsName[p, i] != null && _setAttribsName[p, i] != "") return true;
			}
			return false;
		}

		public bool IsProcessOpen(string name)
		{
			//here we're going to get a list of all running processes on
			//the computer
			try
			{
				foreach (Process clsProcess in Process.GetProcesses())
				{
					//now we're going to see if any of the running processes
					//match the currently running processes. Be sure to not
					//add the .exe to the name you provide, i.e: NOTEPAD,
					//not NOTEPAD.EXE or false is always returned even if
					//notepad is running.
					//Remember, if you have the process running more than once, 
					//say IE open 4 times the loop thr way it is now will close all 4,
					//if you want it to just close the first one it finds
					//then add a return; after the Kill
					if (clsProcess.ProcessName.Contains(name))
					{
						//if the process is found to be running then we
						//return a true
						return true;
					}
				}
			}
			catch (Exception)
			{
				return false;
			}
			
			//otherwise we return a false
			return false;
		}

		private void BtnSaveClick(object sender, EventArgs e)
		{
			if (_fileName == null) return;
			SaveFile();
			if (IsProcessOpen("hl2"))
			{
				MessageBox.Show(Resources.MainForm_BtnSaveClick_I_ve_saved_the_file__but_TF2_seems_to_be_open__To_see_the_changes_you_made__please_restart_the_game_, Resources.MainForm_BtnSaveClick_Nice_job__pardner_, MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
			//if (Osinfo.MajorVersion.ToString() + "." + Osinfo.MinorVersion.ToString() == "6.1") Windows7.DesktopIntegration.Windows7Taskbar.SetProgressState(this.Handle, Windows7Taskbar.ThumbnailProgressState.NoProgress);
		}

		private void BtnSaveAsClick(object sender, EventArgs e) //Save As doesn't work at the moment!!
		{
			if (_fileName == null) return;
			filediagSave.InitialDirectory = "C:\\Program Files\\Steam\\steamapps";
			filediagSave.Filter = Resources.Form1_BtnSaveAsClick_All_files____;
			filediagSave.RestoreDirectory = false;
			DialogResult result = filediagSave.ShowDialog();
			if (result == DialogResult.OK)
			{
				if (_fileName == filediagSave.FileName)
				{
					SaveFile();
					return;
				}
				if (File.Exists(@filediagSave.FileName) && filediagSave.FileName != _fileName) File.Delete(@filediagSave.FileName);
				File.Copy(_fileName, @filediagSave.FileName);
				_fileName = filediagSave.FileName;
				SaveFile();
			}
		}

		private void LinkLabel1LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			string target = linkLabel1.Text;

			try
			{
				Process.Start(target);
			}
			catch
				(
				Win32Exception noBrowser)
			{
				if (noBrowser.ErrorCode == -2147467259) MessageBox.Show(noBrowser.Message);
			}
			catch (Exception other)
			{
				MessageBox.Show(other.Message);
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData)
			{
				case Keys.Control | Keys.S:
					BtnSaveClick(null, null);
					return true;
				case Keys.Control | Keys.O:
					Button1Click(null, null);
					return true;
			}
			return false;
		}

		private void BtnCopyClick(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			string[] file = File.ReadAllLines(_fileName);
			int level = 0;
			string lastline = "";
			_saveStr = "";
			_lastSet = comboSets.SelectedIndex;
			_lastSetItem = comboAddSetItem.SelectedIndex;
			_lastItem = comboName.SelectedIndex;
			bool saving = false;
			foreach (string line in file)
			{
				string temp = line;
				if (line.Contains("{"))
				{
					level++;
					if (!saving) goto end;
				}
				if (line.Contains("}"))
				{
					level--;
					if (!saving) goto end;
				}

				if (IsNumeric(line.Replace("\"", "").Replace("\t", "").Replace(" ", "")) &&
					(lastline.Contains("\t\t}") || lastline.Contains("\t\t{")))
				{
					if (Convert.ToInt32(line.Replace("\"", "").Replace("\t", "").Replace(" ", "")) ==
						comboName.SelectedIndex)
					{
						saving = true;
						_saveNum = comboName.SelectedIndex;
						_saveStr += line + "\r\n";
						goto end;
					}
				}
				if (saving && level == 2 && line.Contains("\t\t}"))
				{
					_saveStr += "\t\t}";
					break;
				}
				if (saving) _saveStr += line + "\r\n";
			end:
				lastline = temp;
			}
			return;
		}

		private void BtnPasteClick(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			if (_saveNum == -1) return;
			var newFile = new StringBuilder();
			if (!File.Exists(_fileName))
			{
				FileStream rofl = File.Create(_fileName);
				rofl.Close();
			}
			string[] file = File.ReadAllLines(_fileName);
			int level = 0;
			string lastline = "";
			bool saving = false;
			bool savd = false;
			foreach (string line in file)
			{
				string temp = line;
				if (savd) goto end;
				if (line.Contains("{"))
				{
					level++;
					if (!saving) goto end;
				}
				if (line.Contains("}"))
				{
					level--;
					if (!saving) goto end;
				}

				if (IsNumeric(line.Replace("\"", "").Replace("\t", "").Replace(" ", "")) &&
					(lastline.Contains("\t\t}") || lastline.Contains("\t\t{")))
				{
					if (Convert.ToInt32(line.Replace("\"", "").Replace("\t", "").Replace(" ", "")) ==
						comboName.SelectedIndex)
					{
						temp = _saveStr.Replace("\"" + _saveNum + "\"", "\"" + comboName.SelectedIndex + "\"");
						saving = true;
						goto end;
					}
				}
				if (saving && level == 2 && line.Contains("\t\t}"))
				{
					saving = false;
					savd = true;
					continue;
				}
				if (saving) continue;
			end:
				newFile.Append(temp + "\r\n");
				lastline = temp;
			}
			try
			{
				var lol = new StreamWriter(_fileName);
				lol.Write(newFile.ToString());
				lol.Close();
			}
			catch
			{
				MessageBox.Show(Resources.Form1_SaveFile_Something_went_wrong_while_saving_,
								Resources.Form1_SaveFile_Who_send_all_these_babies_to_fight__,
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
			}

			ReadFile();
			comboAddSetItem.SelectedIndex = _lastSetItem;
			comboSets.SelectedIndex = _lastSet;
			comboName.SelectedIndex = _lastItem;
		}

		private void ListAllAttribsMouseMove(object sender, MouseEventArgs e)
		{
			var listBox = (ListBox)sender;
			try
			{
				int index = listBox.IndexFromPoint(e.Location);
				if (index > -1 && index < listBox.Items.Count)
				{
					string tipp = listBox.Items[index].ToString();
					int idd = GetAttribId(tipp);
					string tip = ToolTips.FindToolTip(tipp);
					if (tip != _lastTip)
					{
						ListToolTip.SetToolTip(listBox, tip);
						_lastTip = tip;
					}
				}
			}
			catch
			{
				ListToolTip.Hide(listBox);
			}
		}


		private void GridAttribsUserDeletedRow(object sender, DataGridViewRowEventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			if (e == null || _firstSetup) return;
			for (int i = 0; i < NumberOfAttribs; i++)
			{
				_itemAttribs[comboName.SelectedIndex, i] = null;
				_itemAttribsValue[comboName.SelectedIndex, i] = 0;
			}
			foreach (DataGridViewRow roo in grid_attribs.Rows)
			{
				if (roo.Cells[2].Value == null) roo.Cells[2].Value = 0;
				_itemAttribsValue[comboName.SelectedIndex, roo.Index] =
					Converter.ToDouble(roo.Cells[2].Value.ToString());
				_itemAttribs[comboName.SelectedIndex, roo.Index] = roo.Cells[0].Value.ToString();
			}
		}

		private void SearchBoxTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			list_all_attribs.Items.Clear();
			for (int i = 0; i < NumberOfAttribs; i++)
			{
				if (_attribName[i] == null || _attribName[i] == "") continue;
				if (_attribName[i].ToLower().Contains(searchBox.Text.ToLower())) list_all_attribs.Items.Add(_attribName[i]);
			}
		}

		private void ComboNameTextUpdate(object sender, EventArgs e)
		{
			if (_firstSetup) return;
			if (comboName.Text == "") return;
			if (comboName.DroppedDown) return;
			if (comboName.SelectedIndex != -1) _lastSel = comboName.SelectedIndex;
			_name[_lastSel] = comboName.Text.Replace("\r\n", "");
			comboName.Items[_lastSel] = _name[_lastSel];
			comboName.Select(_name[_lastSel].Length, _name[_lastSel].Length);
		}

		private void Button1Click2(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			filediagSave.Title = Resources.Form1_Button1Click2_Select_a_file_to_save_the_item_to;

			filediagSave.Filter = Resources.Form1_Button1Click2_Any_file____;
			filediagSave.RestoreDirectory = false;
			DialogResult result = filediagSave.ShowDialog();
			if (result != DialogResult.OK) return;
			string tmpFile = filediagSave.FileName;
			if (!File.Exists(@tmpFile))
			{
				FileStream rofl = File.Create(@tmpFile);
				rofl.Close();
			}
			string[] file = File.ReadAllLines(_fileName);
			int level = 0;
			string lastline = "";
			_saveStr = "";
			bool saving = false;

			for(int i = 0; i < file.Length; i++)
			{
				string line = file[i];
				string temp = line;
				if (line.Contains("{"))
				{
					level++;
					if (!saving) goto end;
				}
				if (line.Contains("}"))
				{
					level--;
					if (!saving) goto end;
				}
				if(line.Contains("\"name\"") && line.Contains("\"" + comboName.Items[comboName.SelectedIndex].ToString().Replace("\r\n", "") + "\""))
				{
					saving = true;
					_saveNum = comboName.SelectedIndex;
					_saveStr += file[i - 2] + "\r\n";
					_saveStr += file[i - 1] + "\r\n";
					_saveStr += line + "\r\n";
					goto end;
				}
				if (saving && level == 2 && line.Contains("\t\t}"))
				{
					_saveStr += "\t\t}";
					break;
				}
				if (saving) _saveStr += line + "\r\n";
			end:
				lastline = temp;
			}
			var rofll = new StreamWriter(@tmpFile);
			rofll.Write(_saveStr);
			rofll.Close();
			MessageBox.Show("This item has been saved to " + @tmpFile,
							"Item saved",
							MessageBoxButtons.OK,
							MessageBoxIcon.Information);
			return;
		}

		#region Save textboxes

		private void TxtItemClassTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_itemClass[comboName.SelectedIndex] = txt_item_class.Text;
		}

		private void TxtItemNameTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_itemName[comboName.SelectedIndex] = txt_item_name.Text;
		}

		private void TxtItemSlotTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_itemSlot[comboName.SelectedIndex] = txt_item_slot.Text;
		}

		private void TxtItemQualityTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_itemQuality[comboName.SelectedIndex] = txt_item_quality.Text;
		}

		private void TxtBaseitemTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_baseitem[comboName.SelectedIndex] = txt_baseitem.Text;
		}

		private void TxtMinIlevelTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_minIlevel[comboName.SelectedIndex] = txt_min_ilevel.Text;
		}

		private void TxtMaxIlevelTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_maxIlevel[comboName.SelectedIndex] = txt_max_ilevel.Text;
		}

		private void TxtImageInventoryTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_imageInventory[comboName.SelectedIndex] = txt_image_inventory.Text;
		}

		private void TxtImageInventorySizeWTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_imageInventorySizeW[comboName.SelectedIndex] = txt_image_inventory_size_w.Text;
		}

		private void TxtImageInventorySizeHTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_imageInventorySizeH[comboName.SelectedIndex] = txt_image_inventory_size_h.Text;
		}

		private void TxtModelPlayerTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_modelPlayer[comboName.SelectedIndex] = txt_model_player.Text;
		}

		private void TxtAttachToHandsTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_attachToHands[comboName.SelectedIndex] = txt_attach_to_hands.Text;
		}

		private void TxtItemTypeNameTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_itemTypeName[comboName.SelectedIndex] = txt_item_type_name.Text;
		}

		private void TxtCraftClassTextChanged(object sender, EventArgs e)
		{
			if (comboName.SelectedIndex == -1) return;
			_craftClass[comboName.SelectedIndex] = txt_craft_class.Text;
		}

		#endregion


		private void ComboSetsSelectedIndexChanged(object sender, EventArgs e)
		{
			listSetItems.Items.Clear();
			gridSet.Rows.Clear();
			int set = comboSets.SelectedIndex;
			for (int i = 0; i < NumberOfSetItems; i++)
			{
				if (_setItems[set, i] == null || _setItems[set, i] == "") continue;
				listSetItems.Items.Add(_setItems[set, i]);
			}
			textStoreBundle.Text = _setBundle[set];
			textSetName.Text = _setName[set];
			for (int i = 0; i < NumberOfSetAttribs; i++)
			{
				if (_setAttribsName[set, i] == null || _setAttribsName[set, i] == "") continue;
				int n = gridSet.Rows.Add();
				gridSet.Rows[n].Cells[0].Value = _setAttribsName[set, i];
				gridSet.Rows[n].Cells[1].Value = GetAttribClass(_setAttribsName[set, i]);
				gridSet.Rows[n].Cells[2].Value = _setAttribsValue[set, i].ToString().Replace(",", ".");
			}
		}

		private void Button2Click(object sender, EventArgs e)
		{
			if (IsSetItem(comboSets.SelectedIndex, comboAddSetItem.SelectedItem.ToString().Replace("\r\n", "")))
			{
				MessageBox.Show(Resources.MainForm_Button2Click_This_item_already_exists_in_the_set_,
								Resources.Form1_SaveFile_Who_send_all_these_babies_to_fight__,
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
				return;
			}
			if (GetFreeSetItem(comboSets.SelectedIndex) == -1)
			{
				MessageBox.Show(Resources.MainForm_Button2Click_ + NumberOfSetItems + Resources.MainForm_Button2Click__items_at_once_,
								Resources.Form1_SaveFile_Who_send_all_these_babies_to_fight__,
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
				return;
			}
			if (comboAddSetItem.SelectedIndex == -1) return;
			listSetItems.Items.Add(comboAddSetItem.SelectedItem);
			_setItems[comboSets.SelectedIndex, GetFreeSetItem(comboSets.SelectedIndex)] = comboAddSetItem.SelectedItem.ToString().Replace("\r\n", "");
		}

		private bool IsSetItem(int j, string p)
		{
			for (int i = 0; i < NumberOfSetItems; i++)
			{
				if (_setItems[j, i] == p) return true;
			}
			return false;
		}

		private int GetFreeSetItem(int p)
		{
			for (int i = 0; i < NumberOfSetItems; i++)
			{
				if (_setItems[p, i] == "" || _setItems[p, i] == null) return i;
			}
			return -1;
		}

		private void Button3Click(object sender, EventArgs e)
		{
			if (listSetItems.SelectedIndex == -1) return;
			_setItems[comboSets.SelectedIndex, listSetItems.SelectedIndex] = "";
			listSetItems.Items.RemoveAt(listSetItems.SelectedIndex);
			for (int i = comboSets.SelectedIndex + 1; i < NumberOfSetItems; i++)
			{
				_setItems[comboSets.SelectedIndex, i - 1] = _setItems[comboSets.SelectedIndex, i];
			}
		}

		private void BtnAddSetClick(object sender, EventArgs e)
		{
			string resp = Microsoft.VisualBasic.Interaction.InputBox("Please type in a name for the item set.", "All righty then!");
			if (string.IsNullOrEmpty(resp)) return;
			comboSets.Items.Add(resp);
			comboSets.SelectedIndex = comboSets.Items.Count - 1;
		}

		private void TextSetNameTextChanged(object sender, EventArgs e)
		{
			_setName[comboSets.SelectedIndex] = textSetName.Text;
		}

		private void TextStoreBundleTextChanged(object sender, EventArgs e)
		{
			_setBundle[comboSets.SelectedIndex] = textStoreBundle.Text;
		}

		private void GridSetCellValueChanged(object sender, DataGridViewCellEventArgs e)
		{
			if (comboSets.SelectedIndex == -1) return;
			if (e == null) return;
			int i = 0;
			if (e.ColumnIndex == 0 || _firstSetup) return;
			foreach (DataGridViewRow roo in gridSet.Rows)
			{
				if (roo.Cells[2].Value == null) continue;
				_setAttribsValue[comboSets.SelectedIndex, i] = Converter.ToDouble(roo.Cells[2].Value.ToString());
				_setAttribsName[comboSets.SelectedIndex, i] = roo.Cells[0].Value.ToString();
				i++;
			}
		}

		private void MainFormLoad(object sender, EventArgs e)
		{
			Text = Resources.MainForm_MainFormLoad_TF2_Items_Editor_v + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Major + "." + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Minor + " build #" + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Revision;
			englishComboTips.Enabled = false;
			englishTextTip.Enabled = false;
			txt_item_class.Enabled = false;
			txt_craft_class.Enabled = false;
			txt_item_type_name.Enabled = false;
			txt_item_name.Enabled = false;
			txt_item_slot.Enabled = false;
			txt_item_quality.Enabled = false;
			txt_baseitem.Enabled = false;
			txt_min_ilevel.Enabled = false;
			txt_max_ilevel.Enabled = false;
			txt_image_inventory.Enabled = false;
			txt_image_inventory_size_w.Enabled = false;
			txt_image_inventory_size_h.Enabled = false;
			txt_model_player.Enabled = false;
			txt_attach_to_hands.Enabled = false;
			list_all_attribs.Enabled = true;
			list_used_by_classes.Enabled = false;
			list_available_classes.Enabled = false;
			move_left_btn.Enabled = false;
			move_right_btn.Enabled = false;
			grid_attribs.Enabled = false;
			searchBox.Enabled = true;
			comboSets.Enabled = false;
			textSetName.Enabled = false;
			textStoreBundle.Enabled = false;
			comboAddSetItem.Enabled = false;
			listSetItems.Enabled = false;
			gridSet.Enabled = false;
			btnCopy.Enabled = false;
			btnPaste.Enabled = false;
			btnAddSet.Enabled = false;
			btnDelSet.Enabled = false;
			button2.Enabled = false;
			button3.Enabled = false;

			tabControl1.TabPages.Remove(tab_ctx);//Removes work in progress tab from view

			if (ApplicationDeployment.IsNetworkDeployed && ApplicationDeployment.CurrentDeployment.IsFirstRun)
			{
				var cl = new WebClient();
				cl.DownloadStringCompleted += ChangesDownloaded;
				cl.DownloadStringAsync(new Uri("http://tf2itemseditor.googlecode.com/svn/trunk/Updater/changes.txt"));

			}

		}
		private void ChangesDownloaded(object sender, DownloadStringCompletedEventArgs args)
		{
			using (var form = new Changes())
			{
				form.Controls["textBox1"].Text = args.Result;
				form.ShowDialog();
			}
		}
		private void GridSetUserDeletedRow(object sender, DataGridViewRowEventArgs e)
		{
			if (comboSets.SelectedIndex == -1) return;
			if (e == null || _firstSetup) return;
			for (int i = 0; i < NumberOfSetAttribs; i++)
			{
				_setAttribsName[comboSets.SelectedIndex, i] = null;
				_setAttribsValue[comboSets.SelectedIndex, i] = 0;
			}
			foreach (DataGridViewRow roo in gridSet.Rows)
			{
				if (roo.Cells[2].Value == null) roo.Cells[2].Value = 0;
				_setAttribsValue[comboName.SelectedIndex, roo.Index] =
					Converter.ToDouble(roo.Cells[2].Value.ToString());
				_setAttribsName[comboName.SelectedIndex, roo.Index] = roo.Cells[0].Value.ToString();
			}
		}

		private void EnglishOpenClick(object sender, EventArgs e)
		{
			filediagOpen.Title = Resources.MainForm_EnglishOpenClick_Select_tf_english_txt_or_team_fortress_content_gcf;

			filediagOpen.Filter =
				Resources.
					MainForm_EnglishOpenClick_tf_english_txt_tf_english_txt_Team_Fortress_2_content_team_fortress_2_content_gcf;
			filediagOpen.RestoreDirectory = false;
			DialogResult result = filediagOpen.ShowDialog();
			if (result != DialogResult.OK) return;
			if (filediagOpen.FileName.Contains(".gcf"))
			{
				var extract = new Process
				{
					StartInfo =
					{
						FileName = "HLExtract.exe",
						Arguments = "-c -v -p \"" + filediagOpen.FileName +
									"\" -e \"root\\tf\\resource\\tf_english.txt\""
					}
				};
				if (!File.Exists("HLExtract.exe"))
				{
					MessageBox.Show(Resources.Form1_button1_Click_HLExtract_exe_is_missing_from_the_program_folder_,
									Resources.Form1_button1_Click_Who_send_all_these_babies_to_fight__,
									MessageBoxButtons.OK,
									MessageBoxIcon.Error);
					return;
				}
				if (!File.Exists("HLLib.dll"))
				{
					MessageBox.Show(Resources.Form1_button1_Click_HLLib_dll_is_missing_from_the_program_folder_,
									Resources.Form1_button1_Click_Who_send_all_these_babies_to_fight__,
									MessageBoxButtons.OK,
									MessageBoxIcon.Error);
					return;
				}
				try
				{
					//extract.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					extract.Start();
					/* System.IO.WaitForChangedResult resultt;
					string directory = fileName.Replace("items_game.txt", "");
					System.IO.FileSystemWatcher watcher = new System.IO.FileSystemWatcher(directory, "items_game.txt");
					resultt = watcher.WaitForChanged(System.IO.WatcherChangeTypes.All);*/
				}
				catch
				{
					MessageBox.Show(Resources.Form1_button1_Click_Something_went_wrong_when_extracting___,
									Resources.Form1_button1_Click_Who_send_all_these_babies_to_fight__,
									MessageBoxButtons.OK,
									MessageBoxIcon.Error);
					return;
				}
				MessageBox.Show(
					Resources.Form1_button1_Click_Extracted_successfully_to_ + Environment.NewLine +
					filediagOpen.FileName.Replace("team fortress 2 content.gcf", "tf_english.txt") + Environment.NewLine +
					Resources.MainForm_btnEngSave_Click_,
					Resources.Form1_button1_Click_Listen_up_,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				_engfileName = filediagOpen.FileName.Replace("team fortress 2 content.gcf", "tf_english.txt");
				if (!extract.HasExited)
				{
					extract.Kill();
					extract.Close();
				}
			}
			else _engfileName = filediagOpen.FileName;
			ReadEnglishFile();
			englishTextTip.Enabled = true;
			englishComboTips.Enabled = true;
		}
		private void ReadEnglishFile()
		{
			using (var sReader = new StreamReader(_engfileName))
			{
				string line;
				string current = "";
				int tipclass = -1;
				_percent = sReader.BaseStream.Length / (double)100;
				while ((line = sReader.ReadLine()) != null)
				{
					englishProgress.Value = (int)sReader.BaseStream.Position / (int)_percent > 100
												? 100
												: (int)sReader.BaseStream.Position / (int)_percent;
					// if (Osinfo.MajorVersion.ToString() + "." + Osinfo.MinorVersion.ToString() == "6.1") progressReading.SetTaskbarProgress(); //Only show progress bar on the taskbar if using Windows 7
					#region Tips
					Match mTip = Regex.Match(line, @"""Tip_(\d)_(\d*)""\t*""(.*)""");
					if (mTip.Success)
					{
						int tipnum = Convert.ToInt32(mTip.Groups[2].Value) - 1;
						_engTips[Convert.ToInt32(mTip.Groups[1].Value) - 1, tipnum] = mTip.Groups[3].Value;
					}
					Match mAtip = Regex.Match(line, @"""Tip_arena_(\d*)""\t*""(.*)""");
					if (mAtip.Success)
					{
						_engTips[9, Convert.ToInt32(mAtip.Groups[1].Value) - 1] = mAtip.Groups[2].Value;
					}
					#endregion
				}
			}
			englishProgress.Value = 100;
			englishComboTips.SelectedIndex = 0;

		}

		private void ComboBox1SelectedIndexChanged(object sender, EventArgs e)
		{
			if (englishComboTips.SelectedIndex == -1) englishTextTip.Text = "";
			var str = new StringBuilder();
			for (int i = 0; i < NumberOfTips; i++)
			{
				if (_engTips[englishComboTips.SelectedIndex, i] == null || _engTips[englishComboTips.SelectedIndex, i] == "") continue;
					str.AppendLine(_engTips[englishComboTips.SelectedIndex, i]);
			}
			englishTextTip.Text = str.ToString();
		}

		private void EnglishTextTipTextChanged(object sender, EventArgs e)
		{
			if (englishComboTips.SelectedIndex == -1) return;
			for (int i = 0; i < NumberOfTips; i++)
			{
				_engTips[englishComboTips.SelectedIndex, i] = "";
			}
			for (int i = 0; i < englishTextTip.Lines.Length; i++)
			{
				_engTips[englishComboTips.SelectedIndex, i] = englishTextTip.Lines[i];
			}
		}

		private void EnglishSaveClick(object sender, EventArgs e)
		{
			SaveEnglishFile();
		}
		public void SaveEnglishFile()
		{
			var newFile = new StringBuilder();
			string lastline = "";
			if (!File.Exists(_engfileName))
			{
				FileStream create = File.Create(_engfileName);
				create.Close();
			}
			string[] file = File.ReadAllLines(_engfileName);
			foreach (string line in file)
			{
				string temp = line;
				Match mTip = Regex.Match(line, @"""Tip_(\d)_Count""\t*""\d*""");
				if (mTip.Success)
				{
					int tipClass = Convert.ToInt32(mTip.Groups[1].Value) - 1;
					int tp = tipClass + 1;
					int c = 1;
					temp = "";
					for (int i = 0; i < NumberOfTips; i++)
					{
						if (_engTips[tipClass, i] == null || _engTips[tipClass, i] == "") continue;
						temp += "\r\n\"Tip_" + tp + "_" + c + "\"\t\t\t\"" + _engTips[tipClass, i] + "\"";
						c++;
					}
					temp = "\"Tip_" + tp + "_Count\"\t\t\t\"" + (c - 1) + "\"" + temp;
					goto end;
				}
				Match aTip = Regex.Match(line, @"""Tip_arena_Count""\t*""\d*""");
				if (aTip.Success)
					{
						const int tipClass = 9;
						int c = 1;
						temp = "";
						for (int i = 0; i < NumberOfTips; i++)
						{
							if (_engTips[tipClass, i] == null || _engTips[tipClass, i] == "") continue;
							temp += "\r\n\"Tip_arena_" + c + "\"\t\t\t\"" + _engTips[tipClass, i] + "\"";
							c++;
						}
						temp = "\"Tip_arena_Count\"\t\t\t\"" + (c - 1) + "\"" + temp;
						goto end;
					}
					Match tCount = Regex.Match(line, @"""Tip_\d_Count""\t*""\d*""");
					Match tNum   = Regex.Match(line, @"""Tip_\d_\d*""\t*"".*""");
			Match aCount = Regex.Match(line, @"""Tip_arena_Count""\t*""\d*""");
			Match aNum   = Regex.Match(line, @"""Tip_arena_\d*""\t*"".*""");
			if (tCount.Success || tNum.Success || aCount.Success || aNum.Success) continue;

			end:
				newFile.Append(temp + "\r\n");
				// ReSharper disable RedundantAssignment
				lastline = temp;
				 // ReSharper restore RedundantAssignment
			}
			try
			{
				var lol = new StreamWriter(_engfileName);
				lol.Write(newFile.ToString());
				lol.Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show(Resources.Form1_SaveFile_Something_went_wrong_while_saving_ + Environment.NewLine + ex,
					Resources.Form1_SaveFile_Who_send_all_these_babies_to_fight__,
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
			progressReading.Value = 100;
		}

		//CTX LOADING
		private void btn_open_ctx_Click(object sender, EventArgs e)
		{
			filediagOpen.Title = "Select a file to open";

			filediagOpen.Filter = "ctx files|*.ctx|txt files|*.txt|Team Fortress 2 Content|team fortress 2 content.gcf";
			filediagOpen.RestoreDirectory = false;
			DialogResult result = filediagOpen.ShowDialog();
			if (result != DialogResult.OK) return;
			if (filediagOpen.FileName.Contains(".gcf"))
			{
				var extract = new Process
								{
									StartInfo =
										{
											FileName = "HLExtract.exe",
											Arguments = "-c -v -p \"" + filediagOpen.FileName +
														"\" -e \"root\\tf\\scripts\"" + "-d \"" + Path.GetDirectoryName(filediagOpen.FileName) + "\""
										}
								};
				if (!File.Exists("HLExtract.exe"))
				{
					MessageBox.Show(Resources.Form1_button1_Click_HLExtract_exe_is_missing_from_the_program_folder_,
									Resources.Form1_button1_Click_Who_send_all_these_babies_to_fight__,
									MessageBoxButtons.OK,
									MessageBoxIcon.Error);
					return;
				}
				if (!File.Exists("HLLib.dll"))
				{
					MessageBox.Show(Resources.Form1_button1_Click_HLLib_dll_is_missing_from_the_program_folder_,
									Resources.Form1_button1_Click_Who_send_all_these_babies_to_fight__,
									MessageBoxButtons.OK,
									MessageBoxIcon.Error);
					return;
				}
				try
				{
					//extract.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
					extract.Start();
					/* System.IO.WaitForChangedResult resultt;
					string directory = fileName.Replace("items_game.txt", "");
					System.IO.FileSystemWatcher watcher = new System.IO.FileSystemWatcher(directory, "items_game.txt");
					resultt = watcher.WaitForChanged(System.IO.WatcherChangeTypes.All);*/
				}
				catch
				{
					MessageBox.Show(Resources.Form1_button1_Click_Something_went_wrong_when_extracting___,
									Resources.Form1_button1_Click_Who_send_all_these_babies_to_fight__,
									MessageBoxButtons.OK,
									MessageBoxIcon.Error);
					return;
				}
				MessageBox.Show(
					"Successfully extracted all ctx files to " + Environment.NewLine +
					filediagOpen.FileName.Replace("team fortress 2 content.gcf", "") +
					Resources.Form1_button1_Click_,
					Resources.Form1_button1_Click_Listen_up_,
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
				if (!extract.HasExited)
				{
					extract.Kill();
					extract.Close();
				}
			}
			else _ctxFileName = filediagOpen.FileName;
		}
	}
}