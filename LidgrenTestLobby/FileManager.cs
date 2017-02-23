using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace LidgrenTestLobby
{
    /// <summary>
    /// The FileManager can handle saving and loading ISerializable Objects. 
    /// </summary>
    public static class FileManager
    {
        #region Public Functions

        public static bool Save<T>(T obj, string path) where T : ISerializable
        {
            return WriteCustomFile(obj, path);
        }

        public static T Load<T>(string path) where T : ISerializable
        {
            FileInfo fileInfo = new FileInfo(path);
            //If we can't find the file we will create a new Instance of the object of type <T>
            if (!fileInfo.Exists)
            {
                //Debug.Log("File does not exist");
                return Activator.CreateInstance<T>();
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream file = File.Open(path, FileMode.Open);

            //Try to Deserialize the file into an object of type <T>.
            try
            {
                
                T obj = (T)binaryFormatter.Deserialize(file);
                file.Close();
                return obj;
            }
            //When we fail. Create a new Instance of the object of type <T>
            catch (SerializationException se)
            {
                file.Close();
                //Debug.LogError(se);
                return Activator.CreateInstance<T>();
            }
        }

        #endregion

        #region Private Functions

        private static bool WriteCustomFile<T>(T obj, string path) where T : ISerializable
        {
            BinaryFormatter bf = new BinaryFormatter();
            //Create a new FileStream on the defined path. (In this FileStream we can send a stream of data into the file)
            FileStream fileStream = File.Create(path);
            try
            {
                //Serialize the obj and send it into the FileStream.
                bf.Serialize(fileStream, obj);
                fileStream.Close();
                return true;
            }
            catch (SerializationException se)
            {
                Debug.LogError(se);
                return false;
            }
        }

        #endregion
    }
}
