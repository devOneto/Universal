using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class DataService : IDataService
{

    public string[] GetSaveNames(string RelativePath){
        return Directory.GetFiles(RelativePath);
    }

    public T LoadData<T>(string RelativePath)
    {
        try
        {
            using ( FileStream stream = new FileStream(RelativePath, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string fileText = reader.ReadToEnd();
                    T resultObject = JsonConvert.DeserializeObject<T>(fileText);
                    return resultObject;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Não foi possível carregar o save. Error: " + e.Message ); 
            return JsonConvert.DeserializeObject<T>("");
        }
    }

    public bool SaveData<T>(string RelativePath, T Data)
    {
        string path = RelativePath; //TODO: definir uma pasta para salvar
        string serializedResult = "";

        try
        {
            if( File.Exists(path) ) File.Delete(path);
            
            if ( Data != null) serializedResult = JsonConvert.SerializeObject(Data);

            using ( FileStream stream = new FileStream(RelativePath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(serializedResult);
                }
            }

            return true;
        
        } 
        catch (Exception e) 
        { 
            Debug.LogError("Não foi possível salvar. Error: " + e.Message ); 
            return false;    
        }
    }
}