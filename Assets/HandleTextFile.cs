using UnityEngine;
using UnityEditor;
using System.IO;

public class HandleTextFile
{
    public static string path;
    [MenuItem("Tools/Write file")]
    public static void WriteString(string str)
    {

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(str);
        writer.Close();
        /*
        //Re-import the file to update the reference in the editor
        AssetDatabase.ImportAsset(path);
        TextAsset asset = Resources.Load(path) as TextAsset;

        //Print the text from the file*/
    }

    [MenuItem("Tools/Read file")]
    public static void ReadString()
    {

        //Read the text from directly from the test.txt file
        StreamReader reader = new StreamReader(path);
        Debug.Log(reader.ReadToEnd());
        reader.Close();
    }

}