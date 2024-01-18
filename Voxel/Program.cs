using System.Numerics;
using Assimp;
using Auios.QuadTree;
using OpenTK.Graphics.OpenGL;
using Voxel;

var importer = new AssimpContext();
var skullPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Resources", "teapot.obj");
var skullScene = importer.ImportFile(skullPath);

var faces = skullScene.Meshes.SelectMany<Mesh, Face>(mesh => mesh.Faces).ToList();
var vertices = skullScene.Meshes.SelectMany<Mesh, Vector3D>(mesh => mesh.Vertices).ToList();
// var faces = skullScene.Meshes[0].Faces;
// var vertices = skullScene.Meshes[0].Vertices;

var minPointX = vertices.Min(vector => vector.X);
var minPointY = vertices.Min(vector => vector.Y);
var minPointZ = vertices.Min(vector => vector.Z);
var maxPointX = vertices.Max(vector => vector.X);
var maxPointY = vertices.Max(vector => vector.Y);
var maxPointZ = vertices.Max(vector => vector.Z);

var origin = new Vector3(minPointX, minPointY, minPointZ);
var maxPoint = new Vector3(maxPointX, maxPointY, maxPointZ);

var widthX = (maxPoint.X - origin.X);
var heightY = (maxPoint.Y - origin.Y);

var quadTree = new QuadTree<FaceWithMeshIdx>(origin.X, origin.Y, widthX, heightY, new ObjectBound(skullScene.Meshes));

for (int i = 0; i < skullScene.Meshes.Count; ++i)
{
    var facesWithIdx = skullScene.Meshes[i].Faces.Select(face => new FaceWithMeshIdx(face, i)).ToList();
    Console.WriteLine($"Adding {facesWithIdx.Count} faces with idx for mesh {i}");
    quadTree.InsertRange(facesWithIdx);
}


var dims = new[] { 64, 64, 64 };
var deltaX = widthX/dims[0];
var deltaY = heightY/dims[1];
var deltaZ = (maxPointZ - minPointZ)/dims[2];
Console.WriteLine($"{deltaX} {deltaY} {deltaZ}");

var resultCenterPoints = new List<Vector3>();

for (var x = 0; x < dims[0]; x++)
{
    for (var y = 0; y < dims[1]; y++)
    {
        var zList = new List<float>();

        var point = new Vector3(
            origin.X + (x + 0.5f) * deltaX,
            origin.Y + (y + 0.5f) * deltaY,
            origin.Z);
        var ray = new Vector3(0, 0, 1);
        
        var rect = new QuadTreeRect(point.X - 0.5f*deltaX, point.Y-0.5f*deltaY, deltaX, deltaY);
        var foundFaces = quadTree.FindObjects(rect);
        
        foreach (var face in faces)
        {
            // var i1 = skullScene.Meshes[face.meshIdx].Vertices[face.face.Indices[0]];
            // var i2 = skullScene.Meshes[face.meshIdx].Vertices[face.face.Indices[1]];
            // var i3 = skullScene.Meshes[face.meshIdx].Vertices[face.face.Indices[2]];
            
            var i1 = vertices[face.Indices[0]];
            var i2 = vertices[face.Indices[1]];
            var i3 = vertices[face.Indices[2]];

            var a = new Vector3(i1.X, i1.Y, i1.Z);
            var b = new Vector3(i2.X, i2.Y, i2.Z);
            var c = new Vector3(i3.X, i3.Y, i3.Z);

            if (Helper.Rti(point, ray, a, b, c, out var intersectPoint))
            {
                zList.Add(intersectPoint.Z);
            }
        }

        var zc = origin.Z + deltaZ / 2;

        for (var z = 0; z < dims[2]; z++)
        {
            var count = zList.Count(zValue => zValue > zc);

            if (count % 2 == 1)
            {
                resultCenterPoints.Add(new Vector3(point.X, point.Y, zc));
            }

            zc += deltaZ;
        }
    }
}
Console.WriteLine(resultCenterPoints.Count);

var exporter = new ExportHelper();
var voxels = resultCenterPoints.Select(vector => new Voxel.Voxel(vector, deltaX, deltaY, deltaZ)).ToList();
exporter.ExportToObj(voxels, Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Resources", "result.obj"));

var pcexport = new PCExporter();
pcexport.ExportToPC(resultCenterPoints, Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Resources", "result_pc.xyz"));
