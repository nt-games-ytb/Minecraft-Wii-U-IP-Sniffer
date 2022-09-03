using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Minecraft_Wii_U_IP_Sniffer;
using geckou;
using System.IO;
using System.Xml;
using System.Diagnostics;

namespace Minecraft_Wii_U_IP_Sniffer
{
    public partial class MCIPSniffer : Form
    {
        WebClient ipInfo = new WebClient();
        public static GeckoUCore GeckoU;
        public static GeckoUConnect GeckoUConnection;
        public static GeckoUDump GeckoUDump;
        public string nintendoNetwork;
        int numberPlayers;
        string numberPlayersText = "Connected players : ";
        string userIP;
        string userName;
        string userX;
        string userY;
        string userZ;
        string loc;
        int separatorPlace;
        ListViewItem saveInfo;

        public MCIPSniffer()
        {
            InitializeComponent();
        }

        private void MCIPSniffer_Load(object sender, EventArgs e)
        {
            ipInfo.Encoding = Encoding.UTF8;
            userIP = ipInfo.DownloadString("https://ipinfo.io/ip");
            savedInformations.View = View.Details;
            loadXml();

            #region Rest IP text
            IP1.Text = "0";
            IP2.Text = "0";
            IP3.Text = "0";
            IP4.Text = "0";
            IP5.Text = "0";
            IP6.Text = "0";
            IP7.Text = "0";
            IP8.Text = "0";
            #endregion
        }

        #region Connection
        #region IP text
        public bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            if (ipString == "127.0.0.1")
            {
                return false;
            }

            string[] splitValues = ipString.Split('.');

            if (splitValues.Length != 4)
            {
                return false;
            }

            return splitValues.All(r => byte.TryParse(r, out byte tempForParsing));
        }

        private void ipText_TextChanged(object sender, EventArgs e)
        {
            if (ValidateIPv4(ipText.Text) == true)
            {
                connect.Enabled = true;
            }
            else
            {
                connect.Enabled = false;
            }
        }
        #endregion

        #region Buttons
        private void connect_Click(object sender, EventArgs e)
        {
            try
            {
                GeckoU = new GeckoUCore(ipText.Text);
                GeckoU.GUC.Connect();

                GetNintendoNetwork();
                MessageBox.Show("Welcome " + nintendoNetwork.Replace("\0", "") + ",\r\nyou are well connected to Minecraft Wii U IP Sniffer !", "Minecraft Wii U IP Sniffer");

                ipText.Enabled = false;
                connect.Enabled = false;
                disconnect.Enabled = true;
                loadPlayersInformations.Enabled = true;
            }
            catch (IOException ex)
            {
                MessageBox.Show(ex.Message, "Minecraft Wii U IP Sniffer");
            }
            catch (System.Net.Sockets.SocketException)
            {
                MessageBox.Show("Error: your ip is not the right one or you are not connected to the internet !", "Minecraft Wii U IP Sniffer");
            }
            catch
            {
                MessageBox.Show("An unknown error has occurred !", "Minecraft Wii U IP Sniffer");
            }
        }

        private void disconnect_Click(object sender, EventArgs e)
        {
            GeckoU.GUC.Close();

            ipText.Enabled = true;
            connect.Enabled = true;
            disconnect.Enabled = false;
            loadPlayersInformations.Enabled = false;
        }
        #endregion

        #region Get Nintendo Network
        private void GetNintendoNetwork()
        {
            char letter1 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x4E);//[0x10AD1C58] + 0x4E
            char letter2 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x50);
            char letter3 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x52);
            char letter4 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x54);
            char letter5 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x56);
            char letter6 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x58);
            char letter7 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x5A);
            char letter8 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x5C);
            char letter9 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x5E);
            char letter10 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x60);
            char letter11 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x62);
            char letter12 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x64);
            char letter13 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x66);
            char letter14 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x68);
            char letter15 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x6A);
            char letter16 = (char)GeckoU.PeekUInt(GeckoU.PeekUInt(0x10AD1C58) + 0x6C);
            nintendoNetwork = $"{letter1}{letter2}{letter3}{letter4}{letter5}{letter6}{letter7}{letter8}{letter9}{letter10}{letter11}{letter12}{letter13}{letter14}{letter15}{letter16}";
        }
        #endregion
        #endregion

        private void loadPlayersInformations_Click(object sender, EventArgs e)
        {
            #region Clear
            GeckoU.clearString2(0x30000000, 0x30000100);
            GeckoU.clearString2(0x38000000, 0x38000080);

            player1Label.Visible = false;
            player2Label.Visible = false;
            player3Label.Visible = false;
            player4Label.Visible = false;
            player5Label.Visible = false;
            player6Label.Visible = false;
            player7Label.Visible = false;
            player8Label.Visible = false;

            x1Label.Visible = false;
            y1Label.Visible = false;
            z1Label.Visible = false;

            x2Label.Visible = false;
            y2Label.Visible = false;
            z2Label.Visible = false;

            x3Label.Visible = false;
            y3Label.Visible = false;
            z3Label.Visible = false;

            x4Label.Visible = false;
            y4Label.Visible = false;
            z4Label.Visible = false;

            x5Label.Visible = false;
            y5Label.Visible = false;
            z5Label.Visible = false;

            x6Label.Visible = false;
            y6Label.Visible = false;
            z6Label.Visible = false;

            x7Label.Visible = false;
            y7Label.Visible = false;
            z7Label.Visible = false;

            x8Label.Visible = false;
            y8Label.Visible = false;
            z8Label.Visible = false;

            IP1.Visible = false;
            IP2.Visible = false;
            IP3.Visible = false;
            IP4.Visible = false;
            IP5.Visible = false;
            IP6.Visible = false;
            IP7.Visible = false;
            IP8.Visible = false;

            country1.Visible = false;
            country2.Visible = false;
            country3.Visible = false;
            country4.Visible = false;
            country5.Visible = false;
            country6.Visible = false;
            country7.Visible = false;
            country8.Visible = false;

            postal1.Visible = false;
            postal2.Visible = false;
            postal3.Visible = false;
            postal4.Visible = false;
            postal5.Visible = false;
            postal6.Visible = false;
            postal7.Visible = false;
            postal8.Visible = false;

            region1.Visible = false;
            region2.Visible = false;
            region3.Visible = false;
            region4.Visible = false;
            region5.Visible = false;
            region6.Visible = false;
            region7.Visible = false;
            region8.Visible = false;

            city1.Visible = false;
            city2.Visible = false;
            city3.Visible = false;
            city4.Visible = false;
            city5.Visible = false;
            city6.Visible = false;
            city7.Visible = false;
            city8.Visible = false;

            latitude1.Visible = false;
            latitude2.Visible = false;
            latitude3.Visible = false;
            latitude4.Visible = false;
            latitude5.Visible = false;
            latitude6.Visible = false;
            latitude7.Visible = false;
            latitude8.Visible = false;

            longitude1.Visible = false;
            longitude2.Visible = false;
            longitude3.Visible = false;
            longitude4.Visible = false;
            longitude5.Visible = false;
            longitude6.Visible = false;
            longitude7.Visible = false;
            longitude8.Visible = false;

            hostname1.Visible = false;
            hostname2.Visible = false;
            hostname3.Visible = false;
            hostname4.Visible = false;
            hostname5.Visible = false;
            hostname6.Visible = false;
            hostname7.Visible = false;
            hostname8.Visible = false;

            society1.Visible = false;
            society2.Visible = false;
            society3.Visible = false;
            society4.Visible = false;
            society5.Visible = false;
            society6.Visible = false;
            society7.Visible = false;
            society8.Visible = false;

            player1Label.Text = "Player 1";
            player2Label.Text = "Player 2";
            player3Label.Text = "Player 3";
            player4Label.Text = "Player 4";
            player5Label.Text = "Player 5";
            player6Label.Text = "Player 6";
            player7Label.Text = "Player 7";
            player8Label.Text = "Player 8";

            x1Label.Text = "X1";
            y1Label.Text = "Y1";
            z1Label.Text = "Z1";

            x2Label.Text = "X2";
            y2Label.Text = "Y2";
            z2Label.Text = "Z2";

            x3Label.Text = "X3";
            y3Label.Text = "Y3";
            z3Label.Text = "Z3";

            x4Label.Text = "X4";
            y4Label.Text = "Y4";
            z4Label.Text = "Z4";

            x5Label.Text = "X5";
            y5Label.Text = "Y5";
            z5Label.Text = "Z5";

            x6Label.Text = "X6";
            y6Label.Text = "Y6";
            z6Label.Text = "Z6";

            x7Label.Text = "X7";
            y7Label.Text = "Y7";
            z7Label.Text = "Z7";

            x8Label.Text = "X8";
            y8Label.Text = "Y8";
            z8Label.Text = "Z8";
            #endregion

            #region Execute
            //get name
            string code = "9421ffb87c0802a63d40109c3d2003189001004c614ad8e46129c87838800000806a00007d2903a64e800421814300c8812300cc7d2a48505529e8ff418200fc930100283f00026d934100303f40100093a1003c3fa010009321002c63bda0e09361003463182edc93810038635a000c93c100407c7c1b7893e100443bc000003fe030003b20001e3b60000057c91838388100087c6a482e7f0903a6932100243bde0001936100083bff0020936100203bbd00044e800421815c00c8813c00cc810100207d2a4850800100085529e8fe8161000c7c1e4840806100108081001480a1001880c1001c80e10024901fffe0917fffe4907fffe8909fffec90bffff090dffff4911ffff890fffffc911dfffc913a00004180ff78830100288321002c83410030836100348381003883a1003c83c1004083e100448001004c382100487c0803a64e800020";
            GeckoU.makeAssembly(0x03917290, code);
            GeckoU.CallFunction(0x03917290, new uint[] { 0x0 });
            //get cordinates
            string code2 = "9421fff87c0802a63d40109c3d2003189001000c6129c878614ad8e47d2903a6806a0000388000004e800421810300c8812300cc7d2848505529e8ff418200407d2903a63d20380081480000392900103908000880ea000080e7005490e9fff080ea000080e7005890e9fff4814a0000814a005c9149fff84200ffd08001000c382100087c0803a64e80002060000000";
            GeckoU.makeAssembly(0x039173D8, code2);
            GeckoU.CallFunction(0x039173D8, new uint[] { 0x0 });
            //get seed
            string code3 = "9421FFF87C0802A63D200316612968187C0802A67D2903A69001000C4E800421812300342C09000041820024814900F83D20103061293030814A0154816A0004814A000091690004914900008001000C3D20010F61296AE0382100087D2903A67C0803A64E80002060000000";
            GeckoU.makeAssembly(0x03917468, code3);
            GeckoU.CallFunction(0x03917468, new uint[] { 0x0 });
            //get seed
            string code4 = "3D2010973D0010A96129923861080E6C8149000080E80000554A083C810A000080E7028839080001910A000081490000554A083C814A00002C0A000140A200803D0010513CC072796108A7B060C65B9A808800003D00108690C7102C61085B6090871028394710283CA010863CC010878088000060A55B8460C614283CE01087908A000860E72CBC3D00108780A50000610830B490AA000C80C6000090CA001080E7000090EA001481080000910A0018812900005529083C814900002C0A0002418200903D2010563D4010C06129B7B83C6010C081290000606300043C8010C03CA010C03D29D22D6084000881090C1F60A5000C3CC010C03CE010C0910A00003D40010F614A6AE060C6001081690C2F7D4903A660E700143D0010C0916300006108001881490C3F9144000081490C4F9145000081490C5F9146000081490C6F9147000081290C7F912800004e8000203D2010A961290E6C8129000081290288814910002C0A000040A2FF5C39400001914910004BFFFF50";
            GeckoU.makeAssembly(0x039182D0, code4);
            GeckoU.CallFunction(0x039182D0, new uint[] { 0x0 });
            #endregion

            #region Players number
            numberPlayers = 0;
            
            if (GeckoU.PeekUInt(0x30000014) == 0x0)
            { }
            else
            {
                numberPlayers = numberPlayers + 1;
            }
            if (GeckoU.PeekUInt(0x30000034) == 0x0)
            { }
            else
            {
                numberPlayers = numberPlayers + 1;
            }
            if (GeckoU.PeekUInt(0x30000054) == 0x0)
            { }
            else
            {
                numberPlayers = numberPlayers + 1;
            }
            if (GeckoU.PeekUInt(0x30000074) == 0x0)
            { }
            else
            {
                numberPlayers = numberPlayers + 1;
            }
            if (GeckoU.PeekUInt(0x30000094) == 0x0)
            { }
            else
            {
                numberPlayers = numberPlayers + 1;
            }
            if (GeckoU.PeekUInt(0x300000B4) == 0x0)
            { }
            else
            {
                numberPlayers = numberPlayers + 1;
            }
            if (GeckoU.PeekUInt(0x300000D4) == 0x0)
            { }
            else
            {
                numberPlayers = numberPlayers + 1;
            }
            if (GeckoU.PeekUInt(0x300000F4) == 0x0)
            { }
            else
            {
                numberPlayers = numberPlayers + 1;
            }
            numberPlayersLabel.Text = numberPlayersText + numberPlayers;
            #endregion

            #region Players name
            if (numberPlayers >= 1)
            {
                //player1Label.Text = GeckoU.PeekString(16, GeckoU.PeekUInt(0x30000014)) + GeckoU.PeekString(16, GeckoU.PeekUInt(0x30000014) + 0x10);

                if (GeckoU.PeekUInt(0x30000014) <= 0x10000000)
                {
                    uint num0 = GeckoU.PeekUInt(0x30000000 + 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(0x30000000 + 0x4);
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(0x30000000 + 0x6);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(0x30000000 + 0x8);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(0x30000000 + 0xA);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(0x30000000 + 0xC);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(0x30000000 + 0xE);
                    char value5 = (char)num5;
                    player1Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}";
                }
                else
                {
                    uint num0 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) - 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014));
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0x2);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0x4);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0x6);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0x8);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0xA);
                    char value5 = (char)num5;
                    uint num6 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0xC);
                    char value6 = (char)num6;
                    uint num7 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0xE);
                    char value7 = (char)num7;
                    uint num8 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0x10);
                    char value8 = (char)num8;
                    uint num9 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0x12);
                    char value9 = (char)num9;
                    uint num10 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0x14);
                    char value10 = (char)num10;
                    uint num11 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0x16);
                    char value11 = (char)num11;
                    uint num12 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0x18);
                    char value12 = (char)num12;
                    uint num13 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0x1A);
                    char value13 = (char)num13;
                    uint num14 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0x1C);
                    char value14 = (char)num14;
                    uint num15 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000014) + 0x1E);
                    char value15 = (char)num15;
                    player1Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}{value6}{value7}{value8}{value9}{value10}{value11}{value12}{value13}{value14}{value15}";
                }

                userName = player1Label.Text;
            }
            if (numberPlayers >= 2)
            {
                //player2Label.Text = GeckoU.PeekString(16, GeckoU.PeekUInt(0x30000034)) + GeckoU.PeekString(16, GeckoU.PeekUInt(0x30000034) + 0x10);

                if (GeckoU.PeekUInt(0x30000014) == GeckoU.PeekUInt(0x30000034))
                {
                    uint num0 = GeckoU.PeekUInt(0x30000020 + 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(0x30000020 + 0x4);
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(0x30000020 + 0x6);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(0x30000020 + 0x8);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(0x30000020 + 0xA);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(0x30000020 + 0xC);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(0x30000020 + 0xE);
                    char value5 = (char)num5;
                    player2Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}";
                }
                else
                {
                    uint num0 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) - 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034));
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0x2);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0x4);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0x6);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0x8);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0xA);
                    char value5 = (char)num5;
                    uint num6 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0xC);
                    char value6 = (char)num6;
                    uint num7 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0xE);
                    char value7 = (char)num7;
                    uint num8 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0x10);
                    char value8 = (char)num8;
                    uint num9 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0x12);
                    char value9 = (char)num9;
                    uint num10 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0x14);
                    char value10 = (char)num10;
                    uint num11 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0x16);
                    char value11 = (char)num11;
                    uint num12 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0x18);
                    char value12 = (char)num12;
                    uint num13 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0x1A);
                    char value13 = (char)num13;
                    uint num14 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0x1C);
                    char value14 = (char)num14;
                    uint num15 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000034) + 0x1E);
                    char value15 = (char)num15;
                    player2Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}{value6}{value7}{value8}{value9}{value10}{value11}{value12}{value13}{value14}{value15}";
                }
            }
            if (numberPlayers >= 3)
            {
                //player3Label.Text = GeckoU.PeekString(16, GeckoU.PeekUInt(0x30000054)) + GeckoU.PeekString(16, GeckoU.PeekUInt(0x30000054) + 0x10);

                if (GeckoU.PeekUInt(0x30000034) == GeckoU.PeekUInt(0x30000054))
                {
                    uint num0 = GeckoU.PeekUInt(0x30000040 + 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(0x30000040 + 0x4);
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(0x30000040 + 0x6);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(0x30000040 + 0x8);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(0x30000040 + 0xA);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(0x30000040 + 0xC);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(0x30000040 + 0xE);
                    char value5 = (char)num5;
                    player3Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}";
                }
                else
                {
                    uint num0 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) - 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054));
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0x2);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0x4);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0x6);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0x8);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0xA);
                    char value5 = (char)num5;
                    uint num6 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0xC);
                    char value6 = (char)num6;
                    uint num7 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0xE);
                    char value7 = (char)num7;
                    uint num8 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0x10);
                    char value8 = (char)num8;
                    uint num9 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0x12);
                    char value9 = (char)num9;
                    uint num10 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0x14);
                    char value10 = (char)num10;
                    uint num11 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0x16);
                    char value11 = (char)num11;
                    uint num12 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0x18);
                    char value12 = (char)num12;
                    uint num13 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0x1A);
                    char value13 = (char)num13;
                    uint num14 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0x1C);
                    char value14 = (char)num14;
                    uint num15 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000054) + 0x1E);
                    char value15 = (char)num15;
                    player3Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}{value6}{value7}{value8}{value9}{value10}{value11}{value12}{value13}{value14}{value15}";
                }
            }
            if (numberPlayers >= 4)
            {
                //player4Label.Text = GeckoU.PeekString(16, GeckoU.PeekUInt(0x30000074)) + GeckoU.PeekString(16, GeckoU.PeekUInt(0x30000074) + 0x10);

                if (GeckoU.PeekUInt(0x30000054) == GeckoU.PeekUInt(0x30000074))
                {
                    uint num0 = GeckoU.PeekUInt(0x30000060 + 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(0x30000060 + 0x4);
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(0x30000060 + 0x6);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(0x30000060 + 0x8);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(0x30000060 + 0xA);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(0x30000060 + 0xC);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(0x30000060 + 0xE);
                    char value5 = (char)num5;
                    player4Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}";
                }
                else
                {
                    uint num0 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) - 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074));
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0x2);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0x4);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0x6);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0x8);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0xA);
                    char value5 = (char)num5;
                    uint num6 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0xC);
                    char value6 = (char)num6;
                    uint num7 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0xE);
                    char value7 = (char)num7;
                    uint num8 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0x10);
                    char value8 = (char)num8;
                    uint num9 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0x12);
                    char value9 = (char)num9;
                    uint num10 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0x14);
                    char value10 = (char)num10;
                    uint num11 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0x16);
                    char value11 = (char)num11;
                    uint num12 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0x18);
                    char value12 = (char)num12;
                    uint num13 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0x1A);
                    char value13 = (char)num13;
                    uint num14 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0x1C);
                    char value14 = (char)num14;
                    uint num15 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000074) + 0x1E);
                    char value15 = (char)num15;
                    player4Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}{value6}{value7}{value8}{value9}{value10}{value11}{value12}{value13}{value14}{value15}";
                }
            }
            if (numberPlayers >= 5)
            {
                //player5Label.Text = GeckoU.PeekString(16, GeckoU.PeekUInt(0x30000094)) + GeckoU.PeekString(16, GeckoU.PeekUInt(0x30000094) + 0x10);

                if (GeckoU.PeekUInt(0x30000074) == GeckoU.PeekUInt(0x30000094))
                {
                    uint num0 = GeckoU.PeekUInt(0x30000080 + 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(0x30000080 + 0x4);
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(0x30000080 + 0x6);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(0x30000080 + 0x8);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(0x30000080 + 0xA);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(0x30000080 + 0xC);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(0x30000080 + 0xE);
                    char value5 = (char)num5;
                    player5Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}";
                }
                else
                {
                    uint num0 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) - 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094));
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0x2);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0x4);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0x6);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0x8);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0xA);
                    char value5 = (char)num5;
                    uint num6 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0xC);
                    char value6 = (char)num6;
                    uint num7 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0xE);
                    char value7 = (char)num7;
                    uint num8 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0x10);
                    char value8 = (char)num8;
                    uint num9 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0x12);
                    char value9 = (char)num9;
                    uint num10 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0x14);
                    char value10 = (char)num10;
                    uint num11 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0x16);
                    char value11 = (char)num11;
                    uint num12 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0x18);
                    char value12 = (char)num12;
                    uint num13 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0x1A);
                    char value13 = (char)num13;
                    uint num14 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0x1C);
                    char value14 = (char)num14;
                    uint num15 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x30000094) + 0x1E);
                    char value15 = (char)num15;
                    player5Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}{value6}{value7}{value8}{value9}{value10}{value11}{value12}{value13}{value14}{value15}";
                }
            }
            if (numberPlayers >= 6)
            {
                //player6Label.Text = GeckoU.PeekString(16, GeckoU.PeekUInt(0x300000B4)) + GeckoU.PeekString(16, GeckoU.PeekUInt(0x300000B4) + 0x10);

                if (GeckoU.PeekUInt(0x30000094) == GeckoU.PeekUInt(0x300000B4))
                {
                    uint num0 = GeckoU.PeekUInt(0x300000A0 + 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(0x300000A0 + 0x4);
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(0x300000A0 + 0x6);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(0x300000A0 + 0x8);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(0x300000A0 + 0xA);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(0x300000A0 + 0xC);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(0x300000A0 + 0xE);
                    char value5 = (char)num5;
                    player6Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}";
                }
                else
                {
                    uint num0 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) - 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4));
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0x2);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0x4);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0x6);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0x8);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0xA);
                    char value5 = (char)num5;
                    uint num6 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0xC);
                    char value6 = (char)num6;
                    uint num7 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0xE);
                    char value7 = (char)num7;
                    uint num8 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0x10);
                    char value8 = (char)num8;
                    uint num9 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0x12);
                    char value9 = (char)num9;
                    uint num10 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0x14);
                    char value10 = (char)num10;
                    uint num11 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0x16);
                    char value11 = (char)num11;
                    uint num12 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0x18);
                    char value12 = (char)num12;
                    uint num13 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0x1A);
                    char value13 = (char)num13;
                    uint num14 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0x1C);
                    char value14 = (char)num14;
                    uint num15 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000B4) + 0x1E);
                    char value15 = (char)num15;
                    player6Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}{value6}{value7}{value8}{value9}{value10}{value11}{value12}{value13}{value14}{value15}";
                }
            }
            if (numberPlayers >= 7)
            {
                //player7Label.Text = GeckoU.PeekString(16, GeckoU.PeekUInt(0x300000D4)) + GeckoU.PeekString(16, GeckoU.PeekUInt(0x300000D4) + 0x10);

                if (GeckoU.PeekUInt(0x300000B4) == GeckoU.PeekUInt(0x300000D4))
                {
                    uint num0 = GeckoU.PeekUInt(0x300000C0 + 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(0x300000C0 + 0x4);
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(0x300000C0 + 0x6);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(0x300000C0 + 0x8);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(0x300000C0 + 0xA);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(0x300000C0 + 0xC);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(0x300000C0 + 0xE);
                    char value5 = (char)num5;
                    player7Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}";
                }
                else
                {
                    uint num0 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) - 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4));
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0x2);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0x4);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0x6);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0x8);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0xA);
                    char value5 = (char)num5;
                    uint num6 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0xC);
                    char value6 = (char)num6;
                    uint num7 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0xE);
                    char value7 = (char)num7;
                    uint num8 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0x10);
                    char value8 = (char)num8;
                    uint num9 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0x12);
                    char value9 = (char)num9;
                    uint num10 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0x14);
                    char value10 = (char)num10;
                    uint num11 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0x16);
                    char value11 = (char)num11;
                    uint num12 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0x18);
                    char value12 = (char)num12;
                    uint num13 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0x1A);
                    char value13 = (char)num13;
                    uint num14 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0x1C);
                    char value14 = (char)num14;
                    uint num15 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000D4) + 0x1E);
                    char value15 = (char)num15;
                    player7Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}{value6}{value7}{value8}{value9}{value10}{value11}{value12}{value13}{value14}{value15}";
                }
            }
            if (numberPlayers >= 8)
            {
                //player8Label.Text = GeckoU.PeekString(16, GeckoU.PeekUInt(0x300000F4)) + GeckoU.PeekString(16, GeckoU.PeekUInt(0x300000F4) + 0x10);

                if (GeckoU.PeekUInt(0x300000D4) == GeckoU.PeekUInt(0x300000F4))
                {
                    uint num0 = GeckoU.PeekUInt(0x300000E0 + 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(0x300000E0 + 0x4);
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(0x300000E0 + 0x6);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(0x300000E0 + 0x8);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(0x300000E0 + 0xA);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(0x300000E0 + 0xC);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(0x300000E0 + 0xE);
                    char value5 = (char)num5;
                    player8Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}";
                }
                else
                {
                    uint num0 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) - 0x2);
                    char value0 = (char)num0;
                    uint num = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4));
                    char value = (char)num;
                    uint num1 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0x2);
                    char value1 = (char)num1;
                    uint num2 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0x4);
                    char value2 = (char)num2;
                    uint num3 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0x6);
                    char value3 = (char)num3;
                    uint num4 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0x8);
                    char value4 = (char)num4;
                    uint num5 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0xA);
                    char value5 = (char)num5;
                    uint num6 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0xC);
                    char value6 = (char)num6;
                    uint num7 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0xE);
                    char value7 = (char)num7;
                    uint num8 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0x10);
                    char value8 = (char)num8;
                    uint num9 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0x12);
                    char value9 = (char)num9;
                    uint num10 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0x14);
                    char value10 = (char)num10;
                    uint num11 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0x16);
                    char value11 = (char)num11;
                    uint num12 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0x18);
                    char value12 = (char)num12;
                    uint num13 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0x1A);
                    char value13 = (char)num13;
                    uint num14 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0x1C);
                    char value14 = (char)num14;
                    uint num15 = GeckoU.PeekUInt(GeckoU.PeekUInt(0x300000F4) + 0x1E);
                    char value15 = (char)num15;
                    player8Label.Text = $"{value0}{value}{value1}{value2}{value3}{value4}{value5}{value6}{value7}{value8}{value9}{value10}{value11}{value12}{value13}{value14}{value15}";
                }
            }
            #endregion

            #region Players coordinates
            if (numberPlayers >= 1)
            {
                uint x = GeckoU.PeekUInt(0x38000000);
                uint y = GeckoU.PeekUInt(0x38000004);
                uint z = GeckoU.PeekUInt(0x38000008);

                string xText = x.ToString();
                if (x > 0xFFFF)
                {
                    x = 0xFFFFFFFF + x;
                    x = 0xFFFFFFFF - x;
                    xText = "-" + x.ToString();
                }

                string yText = y.ToString();
                if (y > 0xFFFF)
                {
                    y = 0xFFFFFFFF + y;
                    y = 0xFFFFFFFF - y;
                    yText = "-" + y.ToString();
                }

                string zText = z.ToString();
                if (z > 0xFFFF)
                {
                    z = 0xFFFFFFFF + z;
                    z = 0xFFFFFFFF - z;
                    zText = "-" + z.ToString();
                }

                x1Label.Text = xText;
                y1Label.Text = yText;
                z1Label.Text = zText;

                userX = x1Label.Text;
                userY = y1Label.Text;
                userZ = z1Label.Text;
            }
            if (numberPlayers >= 2)
            {
                uint x = GeckoU.PeekUInt(0x38000010);
                uint y = GeckoU.PeekUInt(0x38000014);
                uint z = GeckoU.PeekUInt(0x38000018);

                string xText = x.ToString();
                if (x > 0xFFFF)
                {
                    x = 0xFFFFFFFF + x;
                    x = 0xFFFFFFFF - x;
                    xText = "-" + x.ToString();
                }

                string yText = y.ToString();
                if (y > 0xFFFF)
                {
                    y = 0xFFFFFFFF + y;
                    y = 0xFFFFFFFF - y;
                    yText = "-" + y.ToString();
                }

                string zText = z.ToString();
                if (z > 0xFFFF)
                {
                    z = 0xFFFFFFFF + z;
                    z = 0xFFFFFFFF - z;
                    zText = "-" + z.ToString();
                }

                x2Label.Text = xText;
                y2Label.Text = yText;
                z2Label.Text = zText;
            }
            if (numberPlayers >= 3)
            {
                uint x = GeckoU.PeekUInt(0x38000020);
                uint y = GeckoU.PeekUInt(0x38000024);
                uint z = GeckoU.PeekUInt(0x38000028);

                string xText = x.ToString();
                if (x > 0xFFFF)
                {
                    x = 0xFFFFFFFF + x;
                    x = 0xFFFFFFFF - x;
                    xText = "-" + x.ToString();
                }

                string yText = y.ToString();
                if (y > 0xFFFF)
                {
                    y = 0xFFFFFFFF + y;
                    y = 0xFFFFFFFF - y;
                    yText = "-" + y.ToString();
                }

                string zText = z.ToString();
                if (z > 0xFFFF)
                {
                    z = 0xFFFFFFFF + z;
                    z = 0xFFFFFFFF - z;
                    zText = "-" + z.ToString();
                }

                x3Label.Text = xText;
                y3Label.Text = yText;
                z3Label.Text = zText;
            }
            if (numberPlayers >= 4)
            {
                uint x = GeckoU.PeekUInt(0x38000030);
                uint y = GeckoU.PeekUInt(0x38000034);
                uint z = GeckoU.PeekUInt(0x38000038);

                string xText = x.ToString();
                if (x > 0xFFFF)
                {
                    x = 0xFFFFFFFF + x;
                    x = 0xFFFFFFFF - x;
                    xText = "-" + x.ToString();
                }

                string yText = y.ToString();
                if (y > 0xFFFF)
                {
                    y = 0xFFFFFFFF + y;
                    y = 0xFFFFFFFF - y;
                    yText = "-" + y.ToString();
                }

                string zText = z.ToString();
                if (z > 0xFFFF)
                {
                    z = 0xFFFFFFFF + z;
                    z = 0xFFFFFFFF - z;
                    zText = "-" + z.ToString();
                }

                x4Label.Text = xText;
                y4Label.Text = yText;
                z4Label.Text = zText;
            }
            if (numberPlayers >= 5)
            {
                uint x = GeckoU.PeekUInt(0x38000040);
                uint y = GeckoU.PeekUInt(0x38000044);
                uint z = GeckoU.PeekUInt(0x38000048);

                string xText = x.ToString();
                if (x > 0xFFFF)
                {
                    x = 0xFFFFFFFF + x;
                    x = 0xFFFFFFFF - x;
                    xText = "-" + x.ToString();
                }

                string yText = y.ToString();
                if (y > 0xFFFF)
                {
                    y = 0xFFFFFFFF + y;
                    y = 0xFFFFFFFF - y;
                    yText = "-" + y.ToString();
                }

                string zText = z.ToString();
                if (z > 0xFFFF)
                {
                    z = 0xFFFFFFFF + z;
                    z = 0xFFFFFFFF - z;
                    zText = "-" + z.ToString();
                }

                x5Label.Text = xText;
                y5Label.Text = yText;
                z5Label.Text = zText;
            }
            if (numberPlayers >= 6)
            {
                uint x = GeckoU.PeekUInt(0x38000050);
                uint y = GeckoU.PeekUInt(0x38000054);
                uint z = GeckoU.PeekUInt(0x38000058);

                string xText = x.ToString();
                if (x > 0xFFFF)
                {
                    x = 0xFFFFFFFF + x;
                    x = 0xFFFFFFFF - x;
                    xText = "-" + x.ToString();
                }

                string yText = y.ToString();
                if (y > 0xFFFF)
                {
                    y = 0xFFFFFFFF + y;
                    y = 0xFFFFFFFF - y;
                    yText = "-" + y.ToString();
                }

                string zText = z.ToString();
                if (z > 0xFFFF)
                {
                    z = 0xFFFFFFFF + z;
                    z = 0xFFFFFFFF - z;
                    zText = "-" + z.ToString();
                }

                x6Label.Text = xText;
                y6Label.Text = yText;
                z6Label.Text = zText;
            }
            if (numberPlayers >= 7)
            {
                uint x = GeckoU.PeekUInt(0x38000060);
                uint y = GeckoU.PeekUInt(0x38000064);
                uint z = GeckoU.PeekUInt(0x38000068);

                string xText = x.ToString();
                if (x > 0xFFFF)
                {
                    x = 0xFFFFFFFF + x;
                    x = 0xFFFFFFFF - x;
                    xText = "-" + x.ToString();
                }

                string yText = y.ToString();
                if (y > 0xFFFF)
                {
                    y = 0xFFFFFFFF + y;
                    y = 0xFFFFFFFF - y;
                    yText = "-" + y.ToString();
                }

                string zText = z.ToString();
                if (z > 0xFFFF)
                {
                    z = 0xFFFFFFFF + z;
                    z = 0xFFFFFFFF - z;
                    zText = "-" + z.ToString();
                }

                x7Label.Text = xText;
                y7Label.Text = yText;
                z7Label.Text = zText;
            }
            if (numberPlayers >= 8)
            {
                uint x = GeckoU.PeekUInt(0x38000070);
                uint y = GeckoU.PeekUInt(0x38000074);
                uint z = GeckoU.PeekUInt(0x38000078);

                string xText = x.ToString();
                if (x > 0xFFFF)
                {
                    x = 0xFFFFFFFF + x;
                    x = 0xFFFFFFFF - x;
                    xText = "-" + x.ToString();
                }

                string yText = y.ToString();
                if (y > 0xFFFF)
                {
                    y = 0xFFFFFFFF + y;
                    y = 0xFFFFFFFF - y;
                    yText = "-" + y.ToString();
                }

                string zText = z.ToString();
                if (z > 0xFFFF)
                {
                    z = 0xFFFFFFFF + z;
                    z = 0xFFFFFFFF - z;
                    zText = "-" + z.ToString();
                }

                x8Label.Text = xText;
                y8Label.Text = yText;
                z8Label.Text = zText;
            }
            #endregion

            #region Seed
            ulong seed = GeckoU.PeekULong(0x10303030);
            if (seed > 0xA000000000000000)
            {
                seed = 0xFFFFFFFFFFFFFFFF - seed + 0x1;
                seedText.Text = "Seed : -" + seed.ToString();
            }
            else
            {
                seedText.Text = "Seed : " + seed.ToString();
            }
            #endregion

            #region IP
            #region Read IP
            //IP 1
            if (GeckoU.PeekUInt(0x10C00000) == 0)
            {//You are on your own map (solo) or the game can't find ip
                IP1.Text = userIP; 
            }
            else
            {
                IP1.Text = GeckoU.PeekIP(0x10C00000);
            }

            //IP 2
            if (GeckoU.PeekUInt(0x10C00004) != 0)
            {//Perfect
                IP2.Text = GeckoU.PeekIP(0x10C00004);
            }
            else
            {//Empty or problem
                IP2.Text = "0";
            }

            //IP 3
            if (GeckoU.PeekUInt(0x10C00008) != 0)
            {//Perfect
                IP3.Text = GeckoU.PeekIP(0x10C00008);
            }
            else
            {//Empty or problem
                IP3.Text = "0";
            }

            //IP 4
            if (GeckoU.PeekUInt(0x10C0000C) != 0)
            {//Perfect
                IP4.Text = GeckoU.PeekIP(0x10C0000C);
            }
            else
            {//Empty or problem
                IP4.Text = "0";
            }

            //IP 5
            if (GeckoU.PeekUInt(0x10C00010) != 0)
            {//Perfect
                IP5.Text = GeckoU.PeekIP(0x10C00010);
            }
            else
            {//Empty or problem
                IP5.Text = "0";
            }

            //IP 6
            if (GeckoU.PeekUInt(0x10C00014) != 0)
            {//Perfect
                IP6.Text = GeckoU.PeekIP(0x10C00014);
            }
            else
            {//Empty or problem
                IP6.Text = "0";
            }

            //IP 7
            if (GeckoU.PeekUInt(0x10C00018) != 0)
            {//Perfect
                IP7.Text = GeckoU.PeekIP(0x10C00018);
            }
            else
            {//Empty or problem
                IP7.Text = "0";
            }

            //IP 8
            if (GeckoU.PeekUInt(0x10C00020) != 0)
            {//Perfect
                IP8.Text = GeckoU.PeekIP(0x10C00020);
            }
            else
            {//Empty or problem
                IP8.Text = "0";
            }
            #endregion

            #region User
            if (GeckoU.PeekIP(0x10C00004) == userIP)
            {
                player1Label.Text = player2Label.Text;
                x1Label.Text = x2Label.Text;
                y1Label.Text = y2Label.Text;
                z1Label.Text = z2Label.Text;

                player2Label.Text = userName;
                x2Label.Text = userX;
                y2Label.Text = userY;
                z2Label.Text = userZ;
            }
            else if (GeckoU.PeekIP(0x10C00008) == userIP)
            {
                player1Label.Text = player2Label.Text;
                x1Label.Text = x2Label.Text;
                y1Label.Text = y2Label.Text;
                z1Label.Text = z2Label.Text;
                player2Label.Text = player3Label.Text;
                x2Label.Text = x3Label.Text;
                y2Label.Text = y3Label.Text;
                z2Label.Text = z3Label.Text;

                player3Label.Text = userName;
                x3Label.Text = userX;
                y3Label.Text = userY;
                z3Label.Text = userZ;
            }
            else if (GeckoU.PeekIP(0x10C0000C) == userIP)
            {
                player1Label.Text = player2Label.Text;
                x1Label.Text = x2Label.Text;
                y1Label.Text = y2Label.Text;
                z1Label.Text = z2Label.Text;
                player2Label.Text = player3Label.Text;
                x2Label.Text = x3Label.Text;
                y2Label.Text = y3Label.Text;
                z2Label.Text = z3Label.Text;
                player3Label.Text = player4Label.Text;
                x3Label.Text = x4Label.Text;
                y3Label.Text = y4Label.Text;
                z3Label.Text = z4Label.Text;

                player4Label.Text = userName;
                x4Label.Text = userX;
                y4Label.Text = userY;
                z4Label.Text = userZ;
            }
            else if (GeckoU.PeekIP(0x10C00010) == userIP)
            {
                player1Label.Text = player2Label.Text;
                x1Label.Text = x2Label.Text;
                y1Label.Text = y2Label.Text;
                z1Label.Text = z2Label.Text;
                player2Label.Text = player3Label.Text;
                x2Label.Text = x3Label.Text;
                y2Label.Text = y3Label.Text;
                z2Label.Text = z3Label.Text;
                player3Label.Text = player4Label.Text;
                x3Label.Text = x4Label.Text;
                y3Label.Text = y4Label.Text;
                z3Label.Text = z4Label.Text;
                player4Label.Text = player5Label.Text;
                x4Label.Text = x5Label.Text;
                y4Label.Text = y5Label.Text;
                z4Label.Text = z5Label.Text;

                player5Label.Text = userName;
                x5Label.Text = userX;
                y5Label.Text = userY;
                z5Label.Text = userZ;
            }
            else if (GeckoU.PeekIP(0x10C00014) == userIP)
            {
                player1Label.Text = player2Label.Text;
                x1Label.Text = x2Label.Text;
                y1Label.Text = y2Label.Text;
                z1Label.Text = z2Label.Text;
                player2Label.Text = player3Label.Text;
                x2Label.Text = x3Label.Text;
                y2Label.Text = y3Label.Text;
                z2Label.Text = z3Label.Text;
                player3Label.Text = player4Label.Text;
                x3Label.Text = x4Label.Text;
                y3Label.Text = y4Label.Text;
                z3Label.Text = z4Label.Text;
                player4Label.Text = player5Label.Text;
                x4Label.Text = x5Label.Text;
                y4Label.Text = y5Label.Text;
                z4Label.Text = z5Label.Text;
                player5Label.Text = player6Label.Text;
                x5Label.Text = x6Label.Text;
                y5Label.Text = y6Label.Text;
                z5Label.Text = z6Label.Text;

                player6Label.Text = userName;
                x6Label.Text = userX;
                y6Label.Text = userY;
                z6Label.Text = userZ;
            }
            else if (GeckoU.PeekIP(0x10C00018) == userIP)
            {
                player1Label.Text = player2Label.Text;
                x1Label.Text = x2Label.Text;
                y1Label.Text = y2Label.Text;
                z1Label.Text = z2Label.Text;
                player2Label.Text = player3Label.Text;
                x2Label.Text = x3Label.Text;
                y2Label.Text = y3Label.Text;
                z2Label.Text = z3Label.Text;
                player3Label.Text = player4Label.Text;
                x3Label.Text = x4Label.Text;
                y3Label.Text = y4Label.Text;
                z3Label.Text = z4Label.Text;
                player4Label.Text = player5Label.Text;
                x4Label.Text = x5Label.Text;
                y4Label.Text = y5Label.Text;
                z4Label.Text = z5Label.Text;
                player5Label.Text = player6Label.Text;
                x5Label.Text = x6Label.Text;
                y5Label.Text = y6Label.Text;
                z5Label.Text = z6Label.Text;
                player6Label.Text = player7Label.Text;
                x6Label.Text = x7Label.Text;
                y6Label.Text = y7Label.Text;
                z6Label.Text = z7Label.Text;

                player7Label.Text = userName;
                x7Label.Text = userX;
                y7Label.Text = userY;
                z7Label.Text = userZ;
            }
            else if (GeckoU.PeekIP(0x10C0001C) == userIP)
            {
                player1Label.Text = player2Label.Text;
                x1Label.Text = x2Label.Text;
                y1Label.Text = y2Label.Text;
                z1Label.Text = z2Label.Text;
                player2Label.Text = player3Label.Text;
                x2Label.Text = x3Label.Text;
                y2Label.Text = y3Label.Text;
                z2Label.Text = z3Label.Text;
                player3Label.Text = player4Label.Text;
                x3Label.Text = x4Label.Text;
                y3Label.Text = y4Label.Text;
                z3Label.Text = z4Label.Text;
                player4Label.Text = player5Label.Text;
                x4Label.Text = x5Label.Text;
                y4Label.Text = y5Label.Text;
                z4Label.Text = z5Label.Text;
                player5Label.Text = player6Label.Text;
                x5Label.Text = x6Label.Text;
                y5Label.Text = y6Label.Text;
                z5Label.Text = z6Label.Text;
                player6Label.Text = player7Label.Text;
                x6Label.Text = x7Label.Text;
                y6Label.Text = y7Label.Text;
                z6Label.Text = z7Label.Text;
                player7Label.Text = player8Label.Text;
                x7Label.Text = x8Label.Text;
                y7Label.Text = y8Label.Text;
                z7Label.Text = z8Label.Text;

                player8Label.Text = userName;
                x8Label.Text = userX;
                y8Label.Text = userY;
                z8Label.Text = userZ;
            }
            #endregion

            #region Players place
            if (numberPlayers >= 2)
            {
                if (IP2.Text == "0")//alors déplace nom, x, y, z
                {
                    if (IP3.Text != "0" & IP3.Text != userIP)
                    {
                        player3Label.Text = player2Label.Text;
                        x3Label.Text = x2Label.Text;
                        y3Label.Text = y2Label.Text;
                        z3Label.Text = z2Label.Text;
                    }
                    else if (IP4.Text != "0" & IP4.Text != userIP)
                    {
                        player4Label.Text = player2Label.Text;
                        x4Label.Text = x2Label.Text;
                        y4Label.Text = y2Label.Text;
                        z4Label.Text = z2Label.Text;
                    }
                    else if (IP5.Text != "0" & IP5.Text != userIP)
                    {
                        player5Label.Text = player2Label.Text;
                        x5Label.Text = x2Label.Text;
                        y5Label.Text = y2Label.Text;
                        z5Label.Text = z2Label.Text;
                    }
                    else if (IP6.Text != "0" & IP6.Text != userIP)
                    {
                        player6Label.Text = player2Label.Text;
                        x6Label.Text = x2Label.Text;
                        y6Label.Text = y2Label.Text;
                        z6Label.Text = z2Label.Text;
                    }
                    else if (IP7.Text != "0" & IP7.Text != userIP)
                    {
                        player7Label.Text = player2Label.Text;
                        x7Label.Text = x2Label.Text;
                        y7Label.Text = y2Label.Text;
                        z7Label.Text = z2Label.Text;
                    }
                    else if (IP8.Text != "0" & IP8.Text != userIP)
                    {
                        player8Label.Text = player2Label.Text;
                        x8Label.Text = x2Label.Text;
                        y8Label.Text = y2Label.Text;
                        z8Label.Text = z2Label.Text;
                    }

                    player2Label.Text = "Player 2";
                    x2Label.Text = "X2";
                    y2Label.Text = "Y2";
                    z2Label.Text = "Z2";
                }
            }
            if (numberPlayers >= 3)
            {
                if (IP3.Text == "0")//alors déplace nom, x, y, z
                {
                    if (IP4.Text != "0" & IP4.Text != userIP)
                    {
                        player4Label.Text = player3Label.Text;
                        x4Label.Text = x3Label.Text;
                        y4Label.Text = y3Label.Text;
                        z4Label.Text = z3Label.Text;
                    }
                    else if (IP5.Text != "0" & IP5.Text != userIP)
                    {
                        player5Label.Text = player3Label.Text;
                        x5Label.Text = x3Label.Text;
                        y5Label.Text = y3Label.Text;
                        z5Label.Text = z3Label.Text;
                    }
                    else if (IP6.Text != "0" & IP6.Text != userIP)
                    {
                        player6Label.Text = player3Label.Text;
                        x6Label.Text = x3Label.Text;
                        y6Label.Text = y3Label.Text;
                        z6Label.Text = z3Label.Text;
                    }
                    else if (IP7.Text != "0" & IP7.Text != userIP)
                    {
                        player7Label.Text = player3Label.Text;
                        x7Label.Text = x3Label.Text;
                        y7Label.Text = y3Label.Text;
                        z7Label.Text = z3Label.Text;
                    }
                    else if (IP8.Text != "0" & IP8.Text != userIP)
                    {
                        player8Label.Text = player3Label.Text;
                        x8Label.Text = x3Label.Text;
                        y8Label.Text = y3Label.Text;
                        z8Label.Text = z3Label.Text;
                    }

                    player3Label.Text = "Player 3";
                    x3Label.Text = "X3";
                    y3Label.Text = "Y3";
                    z3Label.Text = "Z3";
                }
            }
            if (numberPlayers >= 4)
            {
                if (IP4.Text == "0")//alors déplace nom, x, y, z
                {
                    if (IP5.Text != "0" & IP5.Text != userIP)
                    {
                        player5Label.Text = player4Label.Text;
                        x5Label.Text = x4Label.Text;
                        y5Label.Text = y4Label.Text;
                        z5Label.Text = z4Label.Text;
                    }
                    else if (IP6.Text != "0" & IP6.Text != userIP)
                    {
                        player6Label.Text = player4Label.Text;
                        x6Label.Text = x4Label.Text;
                        y6Label.Text = y4Label.Text;
                        z6Label.Text = z4Label.Text;
                    }
                    else if (IP7.Text != "0" & IP7.Text != userIP)
                    {
                        player7Label.Text = player4Label.Text;
                        x7Label.Text = x4Label.Text;
                        y7Label.Text = y4Label.Text;
                        z7Label.Text = z4Label.Text;
                    }
                    else if (IP8.Text != "0" & IP8.Text != userIP)
                    {
                        player8Label.Text = player4Label.Text;
                        x8Label.Text = x4Label.Text;
                        y8Label.Text = y4Label.Text;
                        z8Label.Text = z4Label.Text;
                    }

                    player4Label.Text = "Player 4";
                    x4Label.Text = "X4";
                    y4Label.Text = "Y4";
                    z4Label.Text = "Z4";
                }
            }
            if (numberPlayers >= 5)
            {
                if (IP5.Text == "0")//alors déplace nom, x, y, z
                {
                    if (IP6.Text != "0" & IP6.Text != userIP)
                    {
                        player6Label.Text = player5Label.Text;
                        x6Label.Text = x5Label.Text;
                        y6Label.Text = y5Label.Text;
                        z6Label.Text = z5Label.Text;
                    }
                    else if (IP7.Text != "0" & IP7.Text != userIP)
                    {
                        player7Label.Text = player5Label.Text;
                        x7Label.Text = x5Label.Text;
                        y7Label.Text = y5Label.Text;
                        z7Label.Text = z5Label.Text;
                    }
                    else if (IP8.Text != "0" & IP8.Text != userIP)
                    {
                        player8Label.Text = player5Label.Text;
                        x8Label.Text = x5Label.Text;
                        y8Label.Text = y5Label.Text;
                        z8Label.Text = z5Label.Text;
                    }

                    player5Label.Text = "Player 5";
                    x5Label.Text = "X5";
                    y5Label.Text = "Y5";
                    z5Label.Text = "Z5";
                }
            }
            if (numberPlayers >= 6)
            {
                if (IP6.Text == "0")//alors déplace nom, x, y, z
                {
                    if (IP7.Text != "0" & IP7.Text != userIP)
                    {
                        player7Label.Text = player6Label.Text;
                        x7Label.Text = x6Label.Text;
                        y7Label.Text = y6Label.Text;
                        z7Label.Text = z6Label.Text;
                    }
                    else if (IP8.Text != "0" & IP8.Text != userIP)
                    {
                        player8Label.Text = player6Label.Text;
                        x8Label.Text = x6Label.Text;
                        y8Label.Text = y6Label.Text;
                        z8Label.Text = z6Label.Text;
                    }

                    player6Label.Text = "Player 6";
                    x6Label.Text = "X6";
                    y6Label.Text = "Y6";
                    z6Label.Text = "Z6";
                }
            }
            if (numberPlayers >= 7)
            {
                if (IP7.Text == "0")//alors déplace nom, x, y, z
                {
                    if (IP8.Text != "0" & IP8.Text != userIP)
                    {
                        player8Label.Text = player7Label.Text;
                        x8Label.Text = x7Label.Text;
                        y8Label.Text = y7Label.Text;
                        z8Label.Text = z7Label.Text;
                    }

                    player7Label.Text = "Player 7";
                    x7Label.Text = "X7";
                    y7Label.Text = "Y7";
                    z7Label.Text = "Z7";
                }
            }
            #endregion
            #endregion

            #region Warning
            //Old message :
            //MessageBox.Show("As this software is a beta, it contains many problems.\r\nWe have tried to optimize the results for the information as best we can, but information may not be attributed to the correct player.\r\nTo be sure that these are the correct players, they must be in the same order in the map and on the software.\r\nTo have better results, you can disconnect and reconnect on the map.", "Minecraft Wii U IP Sniffer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            MessageBox.Show("As this software is a beta, it contains many problems.\r\nWe have tried to optimize the results for the information as best we can, but information may not be attributed to the correct player.\r\nThe information may not be in order for all players (except you).\r\nTo have better results, you can disconnect and reconnect on the map.", "Minecraft Wii U IP Sniffer", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            #endregion

            #region Show result
            #region Players name and coordinates
            if (player1Label.Text != "Player 1")
            {
                player1Label.Visible = true;
                x1Label.Visible = true;
                y1Label.Visible = true;
                z1Label.Visible = true;
            }
            if (player2Label.Text != "Player 2")
            {
                player2Label.Visible = true;
                x2Label.Visible = true;
                y2Label.Visible = true;
                z2Label.Visible = true;
            }
            if (player3Label.Text != "Player 3")
            {
                player3Label.Visible = true;
                x3Label.Visible = true;
                y3Label.Visible = true;
                z3Label.Visible = true;
            }
            if (player4Label.Text != "Player 4")
            {
                player4Label.Visible = true;
                x4Label.Visible = true;
                y4Label.Visible = true;
                z4Label.Visible = true;
            }
            if (player5Label.Text != "Player 5")
            {
                player5Label.Visible = true;
                x5Label.Visible = true;
                y5Label.Visible = true;
                z5Label.Visible = true;
            }
            if (player6Label.Text != "Player 6")
            {
                player6Label.Visible = true;
                x6Label.Visible = true;
                y6Label.Visible = true;
                z6Label.Visible = true;
            }
            if (player7Label.Text != "Player 7")
            {
                player7Label.Visible = true;
                x7Label.Visible = true;
                y7Label.Visible = true;
                z7Label.Visible = true;
            }
            if (player8Label.Text != "Player 8")
            {
                player8Label.Visible = true;
                x8Label.Visible = true;
                y8Label.Visible = true;
                z8Label.Visible = true;
            }
            #endregion

            #region IP
            if (IP1.Text != "0")
            {
                IP1.Visible = true;

                country1.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP1.Text + "/country").Replace("\n", "");
                country1.Visible = true;

                postal1.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP1.Text + "/postal").Replace("\n", "");
                postal1.Visible = true;

                region1.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP1.Text + "/region").Replace("\n", "");
                region1.Visible = true;

                city1.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP1.Text + "/city").Replace("\n", "");
                city1.Visible = true;

                loc = ipInfo.DownloadString("https://ipinfo.io/" + IP1.Text + "/loc").Replace("\n", "");
                separatorPlace = loc.IndexOf(",");
                latitude1.Text = loc.Substring(0, separatorPlace);
                latitude1.Visible = true;
                longitude1.Text = loc.Substring(separatorPlace + 1);
                longitude1.Visible = true;

                hostname1.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP1.Text + "/hostname").Replace("\n", "");
                hostname1.Visible = true;
                society1.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP1.Text + "/org").Replace("\n", "");
                society1.Visible = true;
            }
            if (IP2.Text != "0")
            {
                IP2.Visible = true;

                country2.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP2.Text + "/country").Replace("\n", "");
                country2.Visible = true;

                postal2.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP2.Text + "/postal").Replace("\n", "");
                postal2.Visible = true;

                region2.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP2.Text + "/region").Replace("\n", "");
                region2.Visible = true;

                city2.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP2.Text + "/city").Replace("\n", "");
                city2.Visible = true;

                loc = ipInfo.DownloadString("https://ipinfo.io/" + IP2.Text + "/loc").Replace("\n", "");
                separatorPlace = loc.IndexOf(",");
                latitude2.Text = loc.Substring(0, separatorPlace);
                latitude2.Visible = true;
                longitude2.Text = loc.Substring(separatorPlace + 1);
                longitude2.Visible = true;

                hostname2.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP2.Text + "/hostname").Replace("\n", "");
                hostname2.Visible = true;
                society2.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP2.Text + "/org").Replace("\n", "");
                society2.Visible = true;
            }
            if (IP3.Text != "0")
            {
                IP3.Visible = true;

                country3.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP3.Text + "/country").Replace("\n", "");
                country3.Visible = true;

                postal3.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP3.Text + "/postal").Replace("\n", "");
                postal3.Visible = true;

                region3.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP3.Text + "/region").Replace("\n", "");
                region3.Visible = true;

                city3.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP3.Text + "/city").Replace("\n", "");
                city3.Visible = true;

                loc = ipInfo.DownloadString("https://ipinfo.io/" + IP3.Text + "/loc").Replace("\n", "");
                separatorPlace = loc.IndexOf(",");
                latitude3.Text = loc.Substring(0, separatorPlace);
                latitude3.Visible = true;
                longitude3.Text = loc.Substring(separatorPlace + 1);
                longitude3.Visible = true;

                hostname3.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP3.Text + "/hostname").Replace("\n", "");
                hostname3.Visible = true;
                society3.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP3.Text + "/org").Replace("\n", "");
                society3.Visible = true;
            }
            if (IP4.Text != "0")
            {
                IP4.Visible = true;

                country4.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP4.Text + "/country").Replace("\n", "");
                country4.Visible = true;

                postal4.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP4.Text + "/postal").Replace("\n", "");
                postal4.Visible = true;

                region4.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP4.Text + "/region").Replace("\n", "");
                region4.Visible = true;

                city4.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP4.Text + "/city").Replace("\n", "");
                city4.Visible = true;

                loc = ipInfo.DownloadString("https://ipinfo.io/" + IP4.Text + "/loc").Replace("\n", "");
                separatorPlace = loc.IndexOf(",");
                latitude4.Text = loc.Substring(0, separatorPlace);
                latitude4.Visible = true;
                longitude4.Text = loc.Substring(separatorPlace + 1);
                longitude4.Visible = true;

                hostname4.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP4.Text + "/hostname").Replace("\n", "");
                hostname4.Visible = true;
                society4.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP4.Text + "/org").Replace("\n", "");
                society4.Visible = true;
            }
            if (IP5.Text != "0")
            {
                IP5.Visible = true;

                country5.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP5.Text + "/country").Replace("\n", "");
                country5.Visible = true;

                postal5.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP5.Text + "/postal").Replace("\n", "");
                postal5.Visible = true;

                region5.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP5.Text + "/region").Replace("\n", "");
                region5.Visible = true;

                city5.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP5.Text + "/city").Replace("\n", "");
                city5.Visible = true;

                loc = ipInfo.DownloadString("https://ipinfo.io/" + IP5.Text + "/loc").Replace("\n", "");
                separatorPlace = loc.IndexOf(",");
                latitude5.Text = loc.Substring(0, separatorPlace);
                latitude5.Visible = true;
                longitude5.Text = loc.Substring(separatorPlace + 1);
                longitude5.Visible = true;

                hostname5.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP5.Text + "/hostname").Replace("\n", "");
                hostname5.Visible = true;
                society5.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP5.Text + "/org").Replace("\n", "");
                society5.Visible = true;
            }
            if (IP6.Text != "0")
            {
                IP6.Visible = true;

                country6.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP6.Text + "/country").Replace("\n", "");
                country6.Visible = true;

                postal6.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP6.Text + "/postal").Replace("\n", "");
                postal6.Visible = true;

                region6.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP6.Text + "/region").Replace("\n", "");
                region6.Visible = true;

                city6.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP6.Text + "/city").Replace("\n", "");
                city6.Visible = true;

                loc = ipInfo.DownloadString("https://ipinfo.io/" + IP6.Text + "/loc").Replace("\n", "");
                separatorPlace = loc.IndexOf(",");
                latitude6.Text = loc.Substring(0, separatorPlace);
                latitude6.Visible = true;
                longitude6.Text = loc.Substring(separatorPlace + 1);
                longitude6.Visible = true;

                hostname6.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP6.Text + "/hostname").Replace("\n", "");
                hostname6.Visible = true;
                society6.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP6.Text + "/org").Replace("\n", "");
                society6.Visible = true;
            }
            if (IP7.Text != "0")
            {
                IP7.Visible = true;

                country7.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP7.Text + "/country").Replace("\n", "");
                country7.Visible = true;

                postal7.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP7.Text + "/postal").Replace("\n", "");
                postal7.Visible = true;

                region7.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP7.Text + "/region").Replace("\n", "");
                region7.Visible = true;

                city7.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP7.Text + "/city").Replace("\n", "");
                city7.Visible = true;

                loc = ipInfo.DownloadString("https://ipinfo.io/" + IP7.Text + "/loc").Replace("\n", "");
                separatorPlace = loc.IndexOf(",");
                latitude7.Text = loc.Substring(0, separatorPlace);
                latitude7.Visible = true;
                longitude7.Text = loc.Substring(separatorPlace + 1);
                longitude7.Visible = true;

                hostname7.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP7.Text + "/hostname").Replace("\n", "");
                hostname7.Visible = true;
                society7.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP7.Text + "/org").Replace("\n", "");
                society7.Visible = true;
            }
            if (IP8.Text != "0")
            {
                IP8.Visible = true;

                country8.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP8.Text + "/country").Replace("\n", "");
                country8.Visible = true;

                postal8.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP8.Text + "/postal").Replace("\n", "");
                postal8.Visible = true;

                region8.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP8.Text + "/region").Replace("\n", "");
                region8.Visible = true;

                city8.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP8.Text + "/city").Replace("\n", "");
                city8.Visible = true;

                loc = ipInfo.DownloadString("https://ipinfo.io/" + IP8.Text + "/loc").Replace("\n", "");
                separatorPlace = loc.IndexOf(",");
                latitude8.Text = loc.Substring(0, separatorPlace);
                latitude8.Visible = true;
                longitude8.Text = loc.Substring(separatorPlace + 1);
                longitude8.Visible = true;

                hostname8.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP8.Text + "/hostname").Replace("\n", "");
                hostname8.Visible = true;
                society8.Text = ipInfo.DownloadString("https://ipinfo.io/" + IP8.Text + "/org").Replace("\n", "");
                society8.Visible = true;
            }
            #endregion

            #region Auto save
            if (saveBox.Checked)
            {
                save();
            }
            #endregion
            #endregion
        }

        #region Save
        private void savePlayersIpInformations_Click(object sender, EventArgs e)
        {
            save();
        }

        private void save()
        {
            if (IP1.Text != "0")
            {   
                saveInfo = savedInformations.Items.Add(player1Label.Text);
                saveInfo.SubItems.Add(IP1.Text);
                saveInfo.SubItems.Add(country1.Text);
                saveInfo.SubItems.Add(postal1.Text);
                saveInfo.SubItems.Add(region1.Text);
                saveInfo.SubItems.Add(city1.Text);
                saveInfo.SubItems.Add(latitude1.Text);
                saveInfo.SubItems.Add(longitude1.Text);
                saveInfo.SubItems.Add(hostname1.Text);
                saveInfo.SubItems.Add(society1.Text);
            }
            if (IP2.Text != "0")
            {
                saveInfo = savedInformations.Items.Add(player2Label.Text);
                saveInfo.SubItems.Add(IP2.Text);
                saveInfo.SubItems.Add(country2.Text);
                saveInfo.SubItems.Add(postal2.Text);
                saveInfo.SubItems.Add(region2.Text);
                saveInfo.SubItems.Add(city2.Text);
                saveInfo.SubItems.Add(latitude2.Text);
                saveInfo.SubItems.Add(longitude2.Text);
                saveInfo.SubItems.Add(hostname2.Text);
                saveInfo.SubItems.Add(society2.Text);
            }
            if (IP3.Text != "0")
            {
                saveInfo = savedInformations.Items.Add(player3Label.Text);
                saveInfo.SubItems.Add(IP3.Text);
                saveInfo.SubItems.Add(country3.Text);
                saveInfo.SubItems.Add(postal3.Text);
                saveInfo.SubItems.Add(region3.Text);
                saveInfo.SubItems.Add(city3.Text);
                saveInfo.SubItems.Add(latitude3.Text);
                saveInfo.SubItems.Add(longitude3.Text);
                saveInfo.SubItems.Add(hostname3.Text);
                saveInfo.SubItems.Add(society3.Text);
            }
            if (IP4.Text != "0")
            {
                saveInfo = savedInformations.Items.Add(player4Label.Text);
                saveInfo.SubItems.Add(IP4.Text);
                saveInfo.SubItems.Add(country4.Text);
                saveInfo.SubItems.Add(postal4.Text);
                saveInfo.SubItems.Add(region4.Text);
                saveInfo.SubItems.Add(city4.Text);
                saveInfo.SubItems.Add(latitude4.Text);
                saveInfo.SubItems.Add(longitude4.Text);
                saveInfo.SubItems.Add(hostname4.Text);
                saveInfo.SubItems.Add(society4.Text);
            }
            if (IP5.Text != "0")
            {
                saveInfo = savedInformations.Items.Add(player5Label.Text);
                saveInfo.SubItems.Add(IP5.Text);
                saveInfo.SubItems.Add(country5.Text);
                saveInfo.SubItems.Add(postal5.Text);
                saveInfo.SubItems.Add(region5.Text);
                saveInfo.SubItems.Add(city5.Text);
                saveInfo.SubItems.Add(latitude5.Text);
                saveInfo.SubItems.Add(longitude5.Text);
                saveInfo.SubItems.Add(hostname5.Text);
                saveInfo.SubItems.Add(society5.Text);
            }
            if (IP6.Text != "0")
            {
                saveInfo = savedInformations.Items.Add(player6Label.Text);
                saveInfo.SubItems.Add(IP6.Text);
                saveInfo.SubItems.Add(country6.Text);
                saveInfo.SubItems.Add(postal6.Text);
                saveInfo.SubItems.Add(region6.Text);
                saveInfo.SubItems.Add(city6.Text);
                saveInfo.SubItems.Add(latitude6.Text);
                saveInfo.SubItems.Add(longitude6.Text);
                saveInfo.SubItems.Add(hostname6.Text);
                saveInfo.SubItems.Add(society6.Text);
            }
            if (IP7.Text != "0")
            {
                saveInfo = savedInformations.Items.Add(player7Label.Text);
                saveInfo.SubItems.Add(IP7.Text);
                saveInfo.SubItems.Add(country7.Text);
                saveInfo.SubItems.Add(postal7.Text);
                saveInfo.SubItems.Add(region7.Text);
                saveInfo.SubItems.Add(city7.Text);
                saveInfo.SubItems.Add(latitude7.Text);
                saveInfo.SubItems.Add(longitude7.Text);
                saveInfo.SubItems.Add(hostname7.Text);
                saveInfo.SubItems.Add(society7.Text);
            }
            if (IP8.Text != "0")
            {
                saveInfo = savedInformations.Items.Add(player8Label.Text);
                saveInfo.SubItems.Add(IP8.Text);
                saveInfo.SubItems.Add(country8.Text);
                saveInfo.SubItems.Add(postal8.Text);
                saveInfo.SubItems.Add(region8.Text);
                saveInfo.SubItems.Add(city8.Text);
                saveInfo.SubItems.Add(latitude8.Text);
                saveInfo.SubItems.Add(longitude8.Text);
                saveInfo.SubItems.Add(hostname8.Text);
                saveInfo.SubItems.Add(society8.Text);
            }

            string xmlCode = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                             "\r\n<savedInformations>";
            for (int i = 0; i < savedInformations.Items.Count; i++)
            {
                xmlCode = xmlCode + "\r\n  <player ip=\"" + savedInformations.Items[i].SubItems[1].Text +
                                    "\" country=\"" + savedInformations.Items[i].SubItems[2].Text +
                                    "\" postal=\"" + savedInformations.Items[i].SubItems[3].Text +
                                    "\" region=\"" + savedInformations.Items[i].SubItems[4].Text + 
                                    "\" city=\"" + savedInformations.Items[i].SubItems[5].Text +
                                    "\" latitude=\"" + savedInformations.Items[i].SubItems[6].Text +
                                    "\" longitude=\"" + savedInformations.Items[i].SubItems[7].Text +
                                    "\" hostname=\"" + savedInformations.Items[i].SubItems[8].Text +
                                    "\" society=\"" + savedInformations.Items[i].SubItems[9].Text + "\"";
                xmlCode = xmlCode + "\r\n    >" + savedInformations.Items[i].Text + "</player>";
            }
            xmlCode = xmlCode + "\r\n</savedInformations>";

            StreamWriter streamWriterIP = new StreamWriter("save.xml");
            streamWriterIP.Write(xmlCode);
            streamWriterIP.Close();
        }
        #endregion

        #region Xml
        private void loadXml()
        {
            try
            {
                XmlDocument file = new XmlDocument();
                file.Load("save.xml");

                XmlNodeList list = file.GetElementsByTagName("player");
                foreach (XmlNode player in list)
                {
                    saveInfo = savedInformations.Items.Add(player.InnerText);
                    saveInfo.SubItems.Add(player.Attributes[0].Value);
                    saveInfo.SubItems.Add(player.Attributes[1].Value);
                    saveInfo.SubItems.Add(player.Attributes[2].Value);
                    saveInfo.SubItems.Add(player.Attributes[3].Value);
                    saveInfo.SubItems.Add(player.Attributes[4].Value);
                    saveInfo.SubItems.Add(player.Attributes[5].Value);
                    saveInfo.SubItems.Add(player.Attributes[6].Value);
                    saveInfo.SubItems.Add(player.Attributes[7].Value);
                    saveInfo.SubItems.Add(player.Attributes[8].Value);
                }
            }
            catch
            {
                MessageBox.Show("The software did not find the save file (save.xml).", "Minecraft Wii U IP Sniffer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void reloadXmlFile_Click(object sender, EventArgs e)
        {
            savedInformations.Items.Clear();
            loadXml();
        }

        private void openXmlFile_Click(object sender, EventArgs e)
        {
            Process.Start("save.xml");
        }
        #endregion

        #region Credits
        private void labelNtGames_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.youtube.com/c/nt-games-ytb");
        }

        private void labelGeckoU_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/XxModZxXWiiPlaza/GeckoU");
        }

        private void labelAlts001_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.youtube.com/channel/UC0Y5yP8fx4UJryk4190p-Bg");
        }

        private void labelKatope_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.youtube.com/channel/UCjIG-NeuMDe7pI1KM6bWTPA");
        }

        private void labelApi_Click(object sender, EventArgs e)
        {
            Process.Start("https://ipinfo.io/");
        }
        #endregion

        #region Copy
        private void savedInformations_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 'c')
            {
                StringBuilder copiedInformations = new StringBuilder();
                ListView.SelectedListViewItemCollection selectedInformations = savedInformations.SelectedItems;

                foreach (ListViewItem item in selectedInformations)
                {
                    copiedInformations.Append("Player : " + item.SubItems[0].Text +
                                           " | Address IP : " + item.SubItems[1].Text +
                                           " | Country : " + item.SubItems[2].Text +
                                           " | Postal : " + item.SubItems[3].Text +
                                           " | Region : " + item.SubItems[4].Text +
                                           " | City : " + item.SubItems[5].Text +
                                           " | Latitude : " + item.SubItems[6].Text +
                                           " | Longitude : " + item.SubItems[7].Text +
                                           " | Hostname : " + item.SubItems[8].Text +
                                           " | Society  : " + item.SubItems[9].Text);
                    copiedInformations.AppendLine();
                }

                Clipboard.SetDataObject(copiedInformations.ToString());
                MessageBox.Show("Selected informations copied !", "Minecraft Wii U IP Sniffer");
            }
        }
        #endregion
    }
}
