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

public class ObjExporter
{

    public static bool MeshToFile(MeshFilter mf)
    {
        bool result = false;

        Mesh m = mf.mesh;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

        StringBuilder sb = new StringBuilder();

        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 v in m.vertices)
        {
            sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in m.normals)
        {
            sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, v.z));
        }
        sb.Append("\n");
        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
        }
        for (int material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");
            sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            sb.Append("usemap ").Append(mats[material].name).Append("\n");

            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                    triangles[i] + 1, triangles[i + 1] + 1, triangles[i + 2] + 1));
            }
        }

#if !UNITY_EDITOR
        var ao = KnownFolders.CameraRoll.CreateFileAsync("point_c.obj", CreationCollisionOption.GenerateUniqueName);
        ao.Completed = async delegate {
            if (ao.Status == Windows.Foundation.AsyncStatus.Completed)
            {
                var file = ao.GetResults();

                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var imageBuffer = Encoding.ASCII.GetBytes(sb.ToString());
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