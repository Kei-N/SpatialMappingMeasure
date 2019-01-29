using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;

#if !UNITY_EDITOR
using Windows.Storage;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
#endif

/// TODO
/// HoloLensアプリ内から、Spatial Mappingで取得したメッシューデータをObjファイル形式で出力する
/// SpatialMapingObserver.cs/SpatialMappingManager.csを使って空間スキャナで取得した空間データソースを取得する
/// MeshSaver.cs/SimpleMeshSerializer.csを使ってメッシュをバイナリにシリアライズしてファイルに保存する
/// 
/// GOAL
/// 出力したObjファイルの差分を取る

/// 処理の流れ
/// [計測モード / メッシュデータ取得モード]を切り替えられるようにする
/// 1. AirTap or ボタンクリック時にメッシュデータ取得開始
/// 2. AirTap or ボタンクリック時にメッシュデータ取得終了
/// 3. obj形式ファイルにして出力する

public class MeshDataController : MonoBehaviour, IInputClickHandler {

    [SerializeField] GameObject spatialMapping;
    [SerializeField] UnityEvent SpatialMappingDisable = new UnityEvent();

    // MRTKのMeshSaver.csのメソッドを使用するとobjファイルが文字化けする
    //    private string MeshFolderName
    //    {
    //        get
    //        {
    //#if !UNITY_EDITOR && UNITY_WSA
    //                return KnownFolders.CameraRoll.Path;
    //#else
    //            return Application.persistentDataPath;
    //#endif
    //        }
    //    }
    //    private string fileName = "office";
    //    private string fileExtension = ".obj";

    void Start()
    {
        InputManager.Instance.AddGlobalListener(this.gameObject);
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        // C点を取得
        var hitObj = GazeManager.Instance.HitObject;
        if (hitObj.name != "PointC") return;
        var pointC = hitObj.transform.position;

        // AirTap時にメッシュデータを保存する
        if (!SaveMesh(pointC)) return; // 自作のロジック

        // SpatialMappingをOFFにする
        SpatialMappingDisable.Invoke();
        hitObj.SetActive(false);

        /* 未使用であるためコメントアウト
        // WorldAnchorをアタッチしたいGameObjectと識別子を指定する
        WorldAnchorManager.Instance.AttachAnchor(gameObject, "anchor01");
        // Anchorを外したいGameObjectを指定する
        WorldAnchorManager.Instance.RemoveAnchor(gameObject);
        */

        // MRTKから拝借(MeshSaver.cs)
        //List<MeshFilter> meshFilters = SpatialMappingManager.Instance.GetMeshFilters();
        //Save(fileName, meshFilters); 
    }

    // メッシュデータをカメラロールにOBJ形式で出力する
    private bool SaveMesh(Vector3 origin)
    {
        StringBuilder modelData = new StringBuilder();

        int offset = 0;
        int count = 0;

        // OriginにC点を代入
        SpatialMappingObserver spatialMappingObserver = SpatialMappingManager.Instance.SurfaceObserver;
        if (!spatialMappingObserver.SetObserverOrigin(origin)) return false;

        // C地点を示すプリミティブオブジェクトを生成する
        GameObject pointC = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        pointC.transform.position = origin;
        //pointC.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        //if(!ObjExporter.MeshToFile(pointC.GetComponent<MeshFilter>())) return false;
        if (!JsonExporter.VectorToJson(origin)) return false;

        Destroy(pointC);

        // Spatial Mappingで取ったMeshFilterを取得する
        List<MeshFilter> meshFilters = SpatialMappingManager.Instance.GetMeshFilters();

        foreach (var filter in meshFilters)
        {
            if (filter != null)
            {
                // メッシュデータはそれぞれのMeshFilterのsharedMeshに入っており、その中のvertices(ジオメトリ頂点)およびtriangles(ポリゴン面要素)を取り出す
                var mesh = filter.sharedMesh;

                modelData.AppendFormat("o object.{0}\n", ++count);

                // ジオメトリ頂点
                foreach (var vertex in mesh.vertices)
                {
                    // ローカル座標をワールド座標に変換する
                    var v = filter.transform.TransformPoint(vertex);
                    modelData.AppendFormat("v {0} {1} {2}\n", -v.x, v.y, v.z); // 左右反転しているためx座標にマイナスを掛ける
                }

                modelData.Append("\n");

                // ポリゴン面要素 (mesh.trianglesの処理が重すぎるためfor文の中で計算するとメモリリークを起こす)
                var meshTriangles = mesh.triangles;
                var meshTrianglesLength = meshTriangles.Length;
                int[] copyMeshTriangles = new int[meshTrianglesLength];
                Array.Copy(meshTriangles, copyMeshTriangles, meshTrianglesLength);
                for (int i = 0; i < meshTrianglesLength; i += 3)
                {
                    modelData.AppendFormat("f {0} {1} {2}\n",
                        copyMeshTriangles[i + 0] + 1 + offset,
                        copyMeshTriangles[i + 1] + 1 + offset,
                        copyMeshTriangles[i + 2] + 1 + offset);
                }
                
                modelData.Append("\n");
                modelData.Append("\n");

                offset += mesh.vertexCount;

            }
        }

#if !UNITY_EDITOR
        var ao = KnownFolders.CameraRoll.CreateFileAsync("ns_office.obj", CreationCollisionOption.GenerateUniqueName);
        ao.Completed = async delegate {
            if (ao.Status == Windows.Foundation.AsyncStatus.Completed)
            {
                var file = ao.GetResults();

                using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var imageBuffer = Encoding.ASCII.GetBytes(modelData.ToString());
                    await stream.WriteAsync(imageBuffer.AsBuffer());
                }

                Debug.Log("write complete");
            }
        };
#endif
        return true;
    }

    // objファイルが文字化けするためコメントアウト
//    private string Save(string fileName, IEnumerable<MeshFilter> meshFilters)
//    {
//        if (string.IsNullOrEmpty(fileName))
//        {
//            throw new ArgumentException("Must specify a valid fileName.");
//        }

//        if (meshFilters == null)
//        {
//            throw new ArgumentNullException("Value of meshFilters cannot be null.");
//        }

//        // Create the mesh file.
//        String folderName = MeshFolderName;
//        Debug.Log(String.Format("Saving mesh file: {0}", Path.Combine(folderName, fileName + fileExtension)));

//        using (Stream stream = OpenFileForWrite(folderName, fileName + fileExtension))
//        {
//            // Serialize and write the meshes to the file.
//            byte[] data = SimpleMeshSerializer.Serialize(meshFilters);
//            stream.Write(data, 0, data.Length);
//            stream.Flush();
//        }

//        Debug.Log("Mesh file saved.");

//        return Path.Combine(folderName, fileName + fileExtension);
//    }

//    private Stream OpenFileForWrite(string folderName, string fileName)
//    {
//        Stream stream = null;

//#if !UNITY_EDITOR && UNITY_WSA
//            Task<Task> task = Task<Task>.Factory.StartNew(
//                            async () =>
//                            {
//                                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(folderName);
//                                StorageFile file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
//                                IRandomAccessStream randomAccessStream = await file.OpenAsync(FileAccessMode.ReadWrite);
//                                stream = randomAccessStream.AsStreamForWrite();
//                            });
//            task.Wait();
//            task.Result.Wait();
//#else
//        stream = new FileStream(Path.Combine(folderName, fileName), FileMode.Create, FileAccess.Write);
//#endif
//        return stream;
//    }
}
