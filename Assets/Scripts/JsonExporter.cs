using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Text;

#if !UNITY_EDITOR
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
#endif

public class JsonExporter : MonoBehaviour
{

    public static bool VectorToJson(Vector3 origin)
    {
        bool result = false;
        // JSONにシリアライズ
        var json = JsonUtility.ToJson(origin);

#if !UNITY_EDITOR
        var ao = KnownFolders.CameraRoll.CreateFileAsync("point_c.json", CreationCollisionOption.GenerateUniqueName);
        ao.Completed = async delegate
        {
            if (ao.Status == Windows.Foundation.AsyncStatus.Completed)
            {
                var file = ao.GetResults();

                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var imageBuffer = Encoding.ASCII.GetBytes(json);
                    await stream.WriteAsync(imageBuffer.AsBuffer());
                }

                Debug.Log("write complete");
            }
        };
#endif
        result = true;
        return result;
    }
}
