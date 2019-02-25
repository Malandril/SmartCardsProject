using System;
using System.Collections;
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
        public List<String> results = new List<string>();

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
            byte[] byBuffIn = new byte[] { 0x00, 0xA4, 0x04, 0x00, 0x07, 0xD2, 0x76, 0x00, 0x00, 0X85, 0x01, 0x01, 0x00 };
            byte[] buffOut = new byte[200];
            int outSize = 300;
            int returnCode = CSC.CSC_ISOCommand(byBuffIn, byBuffIn.Length, buffOut, ref outSize);
            if (returnCode == CSC.RCSC_Ok && outSize > 2 && buffOut[outSize - 2] == 0x90 &&
                buffOut[outSize - 1] == 0x00)
            {
                Console.WriteLine("selected appli : " + CSC.ToStringN(buffOut));
                return;
            }

            throw new Exception("select failed " + returnCode);
        }

        private void select_file(byte[] file)
        {
            byte[] buffOut = new byte[256];
            int outSize = 300;
            byte[] byBuffIn = new byte[] { 0x00, 0xA4, 0x00, 0x0C, 0x02, file[0], file[1] };
            int returnCode = CSC.CSC_ISOCommand(byBuffIn, byBuffIn.Length, buffOut, ref outSize);
            if (returnCode == CSC.RCSC_Ok && outSize > 2 && buffOut[outSize - 2] == 0x90 &&
                buffOut[outSize - 1] == 0x00)
            {
                Console.WriteLine("selected file");
                return;
            }

            throw new Exception("select failed " + returnCode);
        }

        private KeyValuePair<byte[], int> read_binary(Int16 maxLe, Int16 offset = 0x00)
        {
            KeyValuePair<byte[], int> pair;
            byte[] buffOut = new byte[256];
            int outSize = 300;
            byte[] byBuffIn = new byte[] { 0x00, 0xB0, (byte)(offset >> 8), (byte)offset, (byte)maxLe };
            int returnCode = CSC.CSC_ISOCommand(byBuffIn, byBuffIn.Length, buffOut, ref outSize);
            if (returnCode == CSC.RCSC_Ok && outSize > 2 && buffOut[outSize - 2] == 0x90 &&
                buffOut[outSize - 1] == 0x00)
            {
                Console.WriteLine("read binary");
                var res = new byte[outSize - 3];
                for (var i = 1; i < outSize - 2; i++)
                {
                    res[i - 1] = buffOut[i];
                }

                return new KeyValuePair<byte[], int>(res, outSize - 3);
            }
            Console.WriteLine("offset : " + offset + " maxLe : " + maxLe);
            throw new Exception("read binary failed " + returnCode);
        }

        private byte[] slice(byte[] bytes, long start, long end)
        {
            byte[] res = new byte[end - start + 1];
            for (int i = 0; i <= end - start; i++)
            {
                res[i] = bytes[start + i];
            }
            return res;
        }

        private long convertByteArrayToInt(byte[] bytes)
        {
            // If the system architecture is little-endian (that is, little end first),
            // reverse the byte array.
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            long i = BitConverter.ToInt64(bytes, 0);
            return i;
        }

        private byte[] convertIntToByteArray(int integer)
        {
            byte[] bytes = BitConverter.GetBytes(integer);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        private long getNextMessage(byte[] bytes,long startIndex) {

            var header = bytes[++startIndex];
            var bitArray = new BitArray(new byte[] { header });
            var mb = bitArray[7];
            var me = bitArray[6];
            var cf = bitArray[5];
            var sr = bitArray[4];
            var il = bitArray[3];
            byte tnf = (byte)(header & 0x07);

            Console.WriteLine("mb: " + mb + " me: " + me + " cf: " + cf + " sr: " + sr + " il: " + il + " tnf: " + Convert.ToString(tnf, 2).PadLeft(3, '0'));

            //champ type length
            var typeLength = bytes[++startIndex];
            Console.WriteLine("typeLength = " + typeLength);

            //champ payload length
            long payloadLength = 0;
            if (sr)
            {
                payloadLength = bytes[++startIndex];
            }
            else
            {
                var subArray = slice(bytes, startIndex, startIndex + 3);
                payloadLength = convertByteArrayToInt(subArray);
                startIndex += 4;
            }
            //Console.WriteLine("payloadLength = " + payloadLength);

            int idLength = 0;
            if (il)
            {
                idLength = bytes[++startIndex];
            }
            //Console.WriteLine("idLength = " + idLength);
            //champs type
            byte[] type = slice(bytes, startIndex + 1, startIndex + typeLength); //indice à changer pour la suite
            //Console.WriteLine("type : " + CSC.ToStringN(type));
            //Console.WriteLine("type : " + convertByteArrayToInt( type));
            startIndex += typeLength;

            //champs id
            byte[] id = new byte[idLength];
            if (il)
            {
                //id en fonction de idLength
                id = slice(bytes, startIndex + 1, startIndex + idLength);
                startIndex += idLength;
                //Console.WriteLine("id : " + CSC.ToStringN(id));
                results.Add("id : "+ CSC.ToStringN(id));

            }

            

            if (typeLength == 1)
            {
                startIndex++;
                if (type[0] == 0x54)
                {
                    //Console.WriteLine("type is text");
                    //Console.WriteLine("next : " + bytes[startIndex]);
                    var isUTF16Encoded = false;
                    var languageLength = bytes[startIndex];
                    if (isUTF16Encoded)
                    {
                        languageLength--;
                    }
                    var language = "";
                    if (languageLength != 0)
                        language = Encoding.UTF8.GetString(slice(bytes, startIndex, startIndex + languageLength));
                    //Console.WriteLine("language : " + language);

                    var message = "";
                    if (isUTF16Encoded)
                    {
                        message = System.Text.Encoding.Unicode.GetString(slice(bytes, startIndex + languageLength + 1, startIndex + payloadLength - 1));
                        //Console.WriteLine("message utf16 : " + message);
                        results.Add("message utf16: " + message+ ".Language is " + language);

                    }
                    else
                    {
                        message = System.Text.Encoding.UTF8.GetString(slice(bytes, startIndex + languageLength + 1, startIndex + payloadLength - 1));
                        //Console.WriteLine("message utf8 : " + message);
                        results.Add("message utf8 : message = " + message + ", Language = " + language);

                    }
                    startIndex += payloadLength - 1;

                }
                else if (type[0] == 0x55)
                {
                    //Console.WriteLine("type is URI");

                    var UriIdentifier = "";

                    switch (bytes[startIndex])
                    {
                        case 0x00:
                            UriIdentifier = "N/A";
                            break;
                        case 0x01:
                            UriIdentifier = "http://www.";
                            break;
                        case 0x02:
                            UriIdentifier = "https://www.";
                            break;
                        case 0x03:
                            UriIdentifier = "http://";
                            break;
                        case 0x04:
                            UriIdentifier = "https://";
                            break;
                    }
                    //Console.WriteLine("UriIdentifier : " + UriIdentifier);
                    var uri = slice(bytes, startIndex, startIndex + payloadLength - 1);
                    //Console.WriteLine("uri : " + Encoding.UTF8.GetString(uri));
                    startIndex += payloadLength - 1;
                    results.Add("Uri : "+UriIdentifier + Encoding.UTF8.GetString(uri));

                }
                else
                {
                    //Console.WriteLine("type not supported and should probably be URI or Text");
                    results.Add("type not supported and should probably be smart poster");

                }
            }
            else if (typeLength == 2)
            {
                if ((type[1] == 0x70) && (type[0] == 0x53))
                {
                    //Console.WriteLine("type is smart poster");
                    results.Add("smartposter");
                }
                else
                {
                    //Console.WriteLine("type not supported and should probably be smart poster");
                    results.Add("type not supported and should probably be smart poster");

                }
            }
            else
            {
                //Console.WriteLine("type is Raw");
                var message = Encoding.UTF8.GetString(slice(bytes, startIndex, startIndex + payloadLength));
                //Console.WriteLine("message : " + message);
                startIndex += payloadLength-1;
                results.Add("Raw : message = "+message);

            }

            return startIndex;
        }

        private void readContent(byte[] bytes)
        {


            var maxlength = bytes[1];
            Console.WriteLine("maxlength : " + maxlength);
            var me = false;
            long startIndex = 1;
            while (startIndex < maxlength)
            {
                startIndex = getNextMessage(bytes,startIndex);
                Console.WriteLine("startIndex : " + startIndex);

            }

            foreach(String s in results){
                Console.WriteLine(s);
            }

        }


        private void testWriteContent()
        {

            //create global buff in
            //byte[] byBuffIn = new byte[35] { 0x00, 0xD6, 0x00, 0x00, 0x00, 0x34, 0x91, 0x01, 0x11, 0x55, 0x01, 0x70, 0x61, 0x72, 0x61, 0x67, 0x6F, 0x6E, 0x2D, 0x72, 0x66, 0x69, 0x64, 0x2E, 0x63, 0x6F, 0x6D, 0x51, 0x01, 0x04, 0x54, 0x00, 0x50, 0x49, 0x44};
            //byte[] byBuffIn = new byte[10] { 0x00, 0xD6, 0x00, 0x00, 0x05, 0x00, 0x03, 0xD0, 0x00, 0x00 };

            byte[] byBuffIn = new byte[] { 0x00, 0xD6, 0x00, 0x00, 0x2D, 0x00, 0x2B, 0x91, 0x01, 0x0B, 0x55, 0x01, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x6D, 0x11, 0x01, 0x11, 0x54, 0x02, 0x66, 0x72, 0x43, 0x6F, 0x75, 0x63, 0x6F, 0x75, 0x20, 0x54, 0x69, 0x62, 0x6F, 0x77, 0x20, 0x21, 0x51, 0x00, 0x04, 0x31, 0x32, 0x33, 0x34, 0x65, 0xA9 };
            byte[] buffOut = new byte[200];
            int outSize = 300;

            Console.WriteLine("buffIn : " + CSC.ToStringN(byBuffIn));
            int returnCode = CSC.CSC_ISOCommand(byBuffIn, byBuffIn.Length, buffOut, ref outSize);
            Console.WriteLine("buffout : " + CSC.ToStringN(buffOut));


            if (returnCode == CSC.RCSC_Ok && outSize > 2 && buffOut[outSize - 2] == 0x90 &&
                buffOut[outSize - 1] == 0x00)
            {
                Console.WriteLine("have been written");
                return;
            }
            throw new Exception("write failed " + returnCode);


        }

        private void writeContent(int maxLc, string uri, string texte, string data,string protocol)
        {
            byte protocolId = 0x00;
            switch (protocol){
                case "http://www.":
                    protocolId = 0x01;
                    break;
                case "https://www.":
                    protocolId = 0x02;
                    break;
                case "http://":
                    protocolId = 0x03;
                    break;
                case "https://":
                    protocolId = 0x04;
                    break;
            }

            Console.WriteLine("protocolId : " + protocolId);

            byte[] uriBytes = Encoding.UTF8.GetBytes(uri);
            byte[] textBytes = Encoding.UTF8.GetBytes(texte);
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);

            //creating total uri bytes bloc
            //TO DO add uri identifier in parameters of function.
            byte[] totalUriBytes = new byte[5+uriBytes.Length];
            //dansle cas particulier de apple.com
            byte[] uriCommand = new byte[] { 0x91, 0x01, (byte)(uriBytes.Length + 1)/*length*/, 0x55, protocolId};
            uriCommand.CopyTo(totalUriBytes, 0);
            uriBytes.CopyTo(totalUriBytes, uriCommand.Length);
            Console.WriteLine("totalUriBytes : " + CSC.ToStringN(totalUriBytes) + " nbr:" + totalUriBytes.Length);

            //creating total data bytes bloc
            byte[] totalDataBytes = new byte[7+dataBytes.Length];
            byte[] dataCommand = new byte[] {0x11,0x01, (byte)(dataBytes.Length+3)/*length*/, 0x54/*T*/,0x02/*UTF8*/, 0x66, 0x72 };
            dataCommand.CopyTo(totalDataBytes, 0);
            dataBytes.CopyTo(totalDataBytes, dataCommand.Length);
            Console.WriteLine("totalDatabytes : " + CSC.ToStringN(totalDataBytes) + " nbr:"+totalDataBytes.Length);


            //creating raw data
            byte[] totalTextBytes = new byte[textBytes.Length+3];
            byte[] textCommand = new byte[] { 0x51, 0x00, (byte)(textBytes.Length)};
            textCommand.CopyTo(totalTextBytes, 0);
            textBytes.CopyTo(totalTextBytes, textCommand.Length);
            Console.WriteLine("totalTextbytes : " + CSC.ToStringN(totalTextBytes) + " nbr:" + totalTextBytes.Length);


            //data
            byte[] dataToWrite = new byte[totalUriBytes.Length + totalDataBytes.Length + totalTextBytes.Length];
            totalUriBytes.CopyTo(dataToWrite, 0);
            totalDataBytes.CopyTo(dataToWrite, totalUriBytes.Length);
            totalTextBytes.CopyTo(dataToWrite, totalUriBytes.Length + totalDataBytes.Length);

            //command
            byte[] command = new byte[] { 0x00, 0xD6 };
            //offset
            byte[] offset = new byte[] { 0x00, 0x00 };
            //LC ??? wat to do ?
            byte[] Lc = convertIntToByteArray(maxLc);
            Lc = new byte[] { Lc[2], Lc[3] };
            //header creation
            //0x02 est la longueur du champs type qui est ici 0x53, 0x70
            byte[] header = new byte[] { 0x00, (byte)(dataToWrite.Length+5), 0xD1, 0x02, (byte)dataToWrite.Length, 0x54, 0x70 };
            //total length
            byte[] totalLength = new byte[] { (byte)(dataToWrite.Length + 2) , 0x00, (byte)(dataToWrite.Length) };
            Console.WriteLine("head + data : ", header.Length + dataToWrite.Length);

            //create global buff in
            byte[] byBuffIn = new byte[command.Length + offset.Length + totalLength.Length /*+ header.Length*/ + dataToWrite.Length];
            command.CopyTo(byBuffIn, 0);
            offset.CopyTo(byBuffIn, command.Length);
            //header.CopyTo(byBuffIn, command.Length + offset.Length + totalLength.Length);
            totalLength.CopyTo(byBuffIn, command.Length + offset.Length);
            dataToWrite.CopyTo(byBuffIn, command.Length + offset.Length + totalLength.Length /*+ header.Length*/);

            byte[] buffOut = new byte[200];
            int outSize = 300;

            Console.WriteLine("buffIn : " + CSC.ToStringN(byBuffIn));
            int returnCode = CSC.CSC_ISOCommand(byBuffIn, byBuffIn.Length, buffOut, ref outSize);
            Console.WriteLine("buffout : "+CSC.ToStringN(buffOut));

            
            if (returnCode == CSC.RCSC_Ok && outSize > 2 && buffOut[outSize - 2] == 0x90 &&
                buffOut[outSize - 1] == 0x00)
            {
                Console.WriteLine("have been written");
                return;
            }
            throw new Exception("write failed " + returnCode);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            results.Clear();    
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
                select_file(new byte[] { 0xE1, 0x03 });
                var result = read_binary(0x0F);

                byte[] buffOut = result.Key;
                int read = result.Value;
                short maxLe = (short)(buffOut[3] << 8 | buffOut[4]);
                int maxLc = buffOut[5] << 8 | buffOut[6];
                byte[] lid = new byte[] { buffOut[9], buffOut[10] };
                int maxLength = buffOut[11] << 8 | buffOut[12];

                select_file(new byte[] { lid[0], lid[1] });
                Console.WriteLine("maxLength : " + maxLength);
                Console.WriteLine("maxLe : " + maxLe);
                Console.WriteLine("maxLc : " + maxLc);

                //string data = "paragon-rfid.com";
                string data = "Coucou Tibow !";
                string texte = "1234";
                string uri = "google.com";
                string protocol = "http://www.";
                //string protocol = "N/A";

                //writeContent(maxLc, uri, texte, data, protocol);
                //testWriteContent();

                Console.WriteLine("outOfwrite");
                var fileData = new List<byte>();
                for (Int16 i = 0; i < maxLength; i += maxLe)
                {
                    result = read_binary((Int16)Math.Min(maxLe, maxLength - i), i);
                    fileData.AddRange(result.Key);
                }


                /*var test = new byte[] { 0x43, 0x6F, 0x75, 0x63, 0x6F, 0x75, 0x20, 0x54, 0x69, 0x62, 0x6F, 0x77, 0x20,0x21 };
                var test2 = new byte[] { 0x01, 0x67, 0x6F, 0x6F, 0x67, 0x6C, 0x65, 0x2E, 0x63, 0x6F, 0x66,0x72 };
                Console.WriteLine("test : "+ System.Text.Encoding.UTF8.GetString(test));*/

                Console.WriteLine(CSC.ToStringN(fileData.ToArray()) + "\n aa" + fileData.Count);
                readContent(fileData.ToArray());

            }
            catch
            {
                MessageBox.Show("Error on trying do deal with reader");
            }

            AskReaderLib.CSC.Close();
        }
    }


}