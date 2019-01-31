using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using AskReaderLib;

namespace CSharpNETTestASKCSCDLL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            CSC.sCARD_SearchExtTag SearchExtender;
            int Status;
            byte[] ATR;
            ATR = new byte[200];
            int lgATR;
            lgATR = 200;
            int Com = 0;
            int SearchMask;

            txtCom.Text = "";
            txtCard.Text = "";

            try
            {
                CSC.SearchCSC();
                // user can also use line below to speed up coupler connection
                //AskReaderLib.CSC.Open ("COM2");

                // Define type of card to be detected: number of occurence for each loop
                SearchExtender.CONT = 0;
                SearchExtender.ISOB = 2;
                SearchExtender.ISOA = 2;
                SearchExtender.TICK = 0;
                SearchExtender.INNO = 0;
                SearchExtender.MIFARE = 0;
                SearchExtender.MV4k = 0;
                SearchExtender.MV5k = 0;
                SearchExtender.MONO = 0;

                // Define type of card to be detected
                SearchMask = CSC.SEARCH_MASK_ISOB | CSC.SEARCH_MASK_ISOA;
                Status = CSC.SearchCardExt(ref SearchExtender, SearchMask, 1, 20, ref Com, ref lgATR, ATR);
                Console.WriteLine("lgATr " + lgATR);

                Console.WriteLine("Status " + Status);
                if (Status != CSC.RCSC_Ok)
                    txtCom.Text = "Error :" + Status.ToString("X");
                else
                    txtCom.Text = Com.ToString("X");

                if (Com == 2)
                    txtCard.Text = "ISO14443A-4 no Calypso";
                else if (Com == 3)
                    txtCard.Text = "INNOVATRON";
                else if (Com == 4)
                    txtCard.Text = "ISOB14443B-4 Calypso";
                else if (Com == 5)
                    txtCard.Text = "Mifare";
                else if (Com == 6)
                    txtCard.Text = "CTS or CTM";
                else if (Com == 8)
                    txtCard.Text = "ISO14443A-3 ";
                else if (Com == 9)
                    txtCard.Text = "ISOB14443B-4 Calypso";
                else if (Com == 12)
                    txtCard.Text = "ISO14443A-4 Calypso";
                else if (Com == 0x6F)
                    txtCard.Text = "Card not found";
                else
                    txtCard.Text = "";
            }
            catch
            {
                MessageBox.Show("Error on trying do deal with reader");
            }

            CSC.Close();
        }

        private void txtCard_TextChanged(object sender, EventArgs e)
        {
        }

        private void select_appli()
        {
            byte[] byBuffIn = new byte[] {0x00, 0xA4, 0x04, 0x00, 0x07, 0xD2, 0x76, 0x00, 0x00, 0X85, 0x01, 0x01, 0x00};
            byte[] buffOut = new byte[200];
            int outSize = 300;
            int returnCode = CSC.CSC_ISOCommand(byBuffIn, byBuffIn.Length, buffOut, ref outSize);
            if (returnCode == CSC.RCSC_Ok && outSize > 2 && buffOut[outSize - 2] == 0x90 &&
                buffOut[outSize - 1] == 0x00)
            {
                Console.WriteLine("selected appli");
                return;
            }

            throw new Exception("select failed " + returnCode);
        }

        private void select_file(byte[] file)
        {
            byte[] buffOut = new byte[256];
            int outSize = 300;
            byte[] byBuffIn = new byte[] {0x00, 0xA4, 0x00, 0x0C, 0x02, file[0], file[1]};
            int returnCode = CSC.CSC_ISOCommand(byBuffIn, byBuffIn.Length, buffOut, ref outSize);
            if (returnCode == CSC.RCSC_Ok && outSize > 2 && buffOut[outSize - 2] == 0x90 &&
                buffOut[outSize - 1] == 0x00)
            {
                Console.WriteLine("selected appli");
                return;
            }

            throw new Exception("select failed " + returnCode);
        }

        private KeyValuePair<byte[], int> read_binary(Int16 maxLe, Int16 offset = 0x00)
        {
            KeyValuePair<byte[], int> pair;
            byte[] buffOut = new byte[256];
            int outSize = 300;
            byte[] byBuffIn = new byte[] {0x00, 0xB0, (byte) (offset >> 8), (byte) offset, (byte) maxLe};
            int returnCode = CSC.CSC_ISOCommand(byBuffIn, byBuffIn.Length, buffOut, ref outSize);
            if (returnCode == CSC.RCSC_Ok && outSize > 2 && buffOut[outSize - 2] == 0x90 &&
                buffOut[outSize - 1] == 0x00)
            {
                Console.WriteLine("read binary");
                var res = new byte[outSize -3];
                for (var i = 1; i < outSize - 2; i++)
                {
                    res[i -1] = buffOut[i];
                }

                return new KeyValuePair<byte[], int>(res, outSize-3);
            }

            throw new Exception("read binary failed " + returnCode);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CSC.sCARD_SearchExtTag SearchExtender;
            int Status;
            byte[] ATR;
            ATR = new byte[200];
            int lgATR;
            lgATR = 200;
            int Com = 0;
            int SearchMask;

            txtCom.Text = "";
            txtCard.Text = "";

            try
            {
                CSC.SearchCSC();
                // user can also use line below to speed up coupler connection
                //AskReaderLib.CSC.Open ("COM2");

                // Define type of card to be detected: number of occurence for each loop
                SearchExtender.CONT = 0;
                SearchExtender.ISOB = 2;
                SearchExtender.ISOA = 2;
                SearchExtender.TICK = 0;
                SearchExtender.INNO = 0;
                SearchExtender.MIFARE = 0;
                SearchExtender.MV4k = 0;
                SearchExtender.MV5k = 0;
                SearchExtender.MONO = 0;
                Status = CSC.CSC_EHP_PARAMS_EXT(1, 1, 0, 0, 0, 0, 0, 0, null, 0, 0);
                // Define type of card to be detected
                SearchMask = CSC.SEARCH_MASK_ISOB | CSC.SEARCH_MASK_ISOA;
                Status = CSC.SearchCardExt(ref SearchExtender, SearchMask, 1, 20, ref Com, ref lgATR, ATR);
                Console.WriteLine("lgATr " + lgATR);
                Console.WriteLine("SearchExtender " + SearchExtender.ISOA + " " + SearchExtender.ISOB);
                Console.WriteLine("ATR", CSC.ToStringN(ATR));
                Console.WriteLine("Status " + Status);
                if (Status != CSC.RCSC_Ok)
                    txtCom.Text = "Error :" + Status.ToString("X");
                else
                    txtCom.Text = Com.ToString("X");
                if (Com == 2)
                    txtCard.Text = "ISO14443A-4 no Calypso";
                else if (Com == 3)
                    txtCard.Text = "INNOVATRON";
                else if (Com == 4)
                    txtCard.Text = "ISOB14443B-4 Calypso";
                else if (Com == 5)
                    txtCard.Text = "Mifare";
                else if (Com == 6)
                    txtCard.Text = "CTS or CTM";
                else if (Com == 8)
                    txtCard.Text = "ISO14443A-3 ";
                else if (Com == 9)
                    txtCard.Text = "ISOB14443B-4 Calypso";
                else if (Com == 12)
                    txtCard.Text = "ISO14443A-4 Calypso";
                else if (Com == 0x6F)
                    txtCard.Text = "Card not found";
                else
                    txtCard.Text = "";


                select_appli();
                select_file(new byte[] {0xE1, 0x03});
                var result = read_binary(0x0F);

                byte[] buffOut = result.Key;
                int read = result.Value;
                short maxLe = (short) (buffOut[3] << 8 | buffOut[4]);
                int maxLc = buffOut[6] << 8 | buffOut[7];
                byte[] lid = new byte[] {buffOut[9], buffOut[10]};
                int maxLength = buffOut[11] << 8 | buffOut[12];

                select_file(new byte[] {lid[0], lid[1]});

                var fileData = new List<byte>();
                for (Int16 i = 0; i < maxLength; i += maxLe)
                {
                    result = read_binary((Int16) maxLe,  i);
                    fileData.AddRange(result.Key);
                }

                Console.WriteLine(CSC.ToStringN(fileData.ToArray()) + "\n aa" + fileData.Count);
            }
            catch
            {
                MessageBox.Show("Error on trying do deal with reader");
            }

            AskReaderLib.CSC.Close();
        }
    }
}