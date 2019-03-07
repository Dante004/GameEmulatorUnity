using Menu.Scripts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class RomGame
{
    public string title;
    public bool gameBoyColor;
    public int lincenseCode;
    public bool gameboy;
    public RomType romType;
    public int romSize;
    public int romBanks;
    public int ramSize;
    public int ramBanks;
    public bool japanese;
    public int oldLicenseCode;
    public int maskRomVersion;
    public int checksum;
    public int actualChecksum;
    public int headerChecksum;
    public int actualHeaderChecksum;
    public bool noVerticalBlankInterruptHandler;
    public bool noLCDCStatusInterruptHandler;
    public bool noTimerOverflowInterruptHandler;
    public bool noSerialTransferCompletionInterruptHandler;
    public bool noHighToLowOfP10ToP13InterruptHandler;

    public RomGame(byte[] fileData)
    {
        title = ExtractGameTitle(fileData);
        gameBoyColor = fileData[0x0143] == 0x80;
        lincenseCode = (((int)fileData[0x0144]) << 4 | fileData[0x0145]);
        gameboy = fileData[0x0146] == 0x00;
        romType = (RomType)fileData[0x0147];

        switch (fileData[0x0148])
        {
            case 0x00:
                romSize = 32 * 1024;
                romBanks = 2;
                break;
            case 0x01:
                romSize = 64 * 1024;
                romBanks = 4;
                break;
            case 0x02:
                romSize = 128 * 1024;
                romBanks = 8;
                break;
            case 0x03:
                romSize = 256 * 1024;
                romBanks = 16;
                break;
            case 0x04:
                romSize = 512 * 1024;
                romBanks = 32;
                break;
            case 0x05:
                romSize = 1024 * 1024;
                romBanks = 32;
                break;
            case 0x06:
                romSize = 2 * 1024 * 1024;
                romBanks = 64;
                break;
            case 0x52:
                romSize = 1179648;
                romBanks = 72;
                break;
            case 0x53:
                romSize = 1310720;
                romBanks = 80;
                break;
            case 0x54:
                romSize = 1572864;
                romBanks = 96;
                break;
            
        }
        switch (fileData[0x0149])
        {
            case 0x00:
                ramSize = 0;
                ramBanks = 0;
                break;
            case 0x01:
                ramSize = 2 * 1024;
                ramBanks = 1;
                break;
            case 0x02:
                ramSize = 8 * 1024;
                ramBanks = 1;
                break;
            case 0x03:
                ramSize = 32 * 1024;
                ramBanks = 4;
                break;
            case 0x04:
                ramSize = 128 * 1024;
                ramBanks = 16;
                break;
        }
        japanese = fileData[0x014A] == 0x00;
        oldLicenseCode = fileData[0x014B];
        maskRomVersion = fileData[0x019C];

        headerChecksum = fileData[0x014D];
        for (int i = 0x0134; i <=0x014C ; ++i)
        {
            actualHeaderChecksum = actualHeaderChecksum - fileData[i] - 1;
            
        }
        actualHeaderChecksum &= 0xFF;
        checksum = (((int)fileData[0x014E]) << 8 | fileData[0x014F]);
        for (int i = 0; i < fileData.Length; ++i)
        {
            if (i!=0x014E&&i!=0x014F)
            {
                actualChecksum += fileData[i];
            }
        }
        actualChecksum &= 0xFFFF;

        noVerticalBlankInterruptHandler = fileData[0x0040] == 0xD9;
        noLCDCStatusInterruptHandler = fileData[0x0048] == 0xD9;
        noTimerOverflowInterruptHandler = fileData[0x0050] == 0xD9;
        noSerialTransferCompletionInterruptHandler = fileData[0x0058] == 0xD9;
        noHighToLowOfP10ToP13InterruptHandler = fileData[0x0060] == 0xD9;

    }
   
    public string ExtractGameTitle(byte[] fileData)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0x0134; i <= 0x0142; ++i)
        {
            if (fileData[i] == 0x00)
            {
                break;
            }
            sb.Append((char)fileData[i]);

        }
        return sb.ToString();
    }
    public override string ToString()
    {
        return "title = " + title + "\n"
           + "game boy color game = " + gameBoyColor + "\n"
           + "license code = " + lincenseCode + "\n"
           + "game boy = " + gameboy + "\n"
           + "rom type = " + romType + "\n"
           + "rom size = " + romSize + "\n"
           + "rom banks = " + romBanks + "\n"
           + "ram size = " + ramSize + "\n"
           + "ram banks = " + ramBanks + "\n"
           + "japanese = " + japanese + "\n"
           + "old license code = " + oldLicenseCode + "\n"
           + "mask rom version = " + maskRomVersion + "\n"
           + "header checksum = " + headerChecksum + "\n"
           + "actual header checksum = " + actualHeaderChecksum + "\n"
           + "checksum = " + checksum + "\n"
           + "actual checksum = " + actualChecksum + "\n"
           + "no vertical blank interrupt handler = " + noVerticalBlankInterruptHandler + "\n"
           + "no lcd status interrupt handler = " + noLCDCStatusInterruptHandler + "\n"
           + "no timer overflow interrupt handler = " + noTimerOverflowInterruptHandler + "\n"
           + "no serial transfer completion interrupt handler = " + noSerialTransferCompletionInterruptHandler + "\n"
           + "no high to lower of P10-P13 interrupt handler = " + noHighToLowOfP10ToP13InterruptHandler + "\n";
    }
}
class ROMLoader
{
    public static RomGame Load(string fileName)
    {
        FileInfo fileInfo = new FileInfo(fileName);
        byte[] fileData = new byte[fileInfo.Length];
        FileStream fileStream = fileInfo.OpenRead();
        fileStream.Read(fileData, 0, fileData.Length);
        fileStream.Close();

        return new RomGame(fileData);
    }
}