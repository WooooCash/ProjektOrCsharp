using System.Numerics;
using System.Text;
using Assimp;

namespace Voxel;

public class Voxel
{
    private readonly Vector3 _center;
    private readonly float _sizeX;
    private readonly float _sizeY;
    private readonly float _sizeZ;

    public Voxel(Vector3 center, float sizeX, float sizeY, float sizeZ)
    {
        _center = center;
        _sizeX = sizeX;
        _sizeY = sizeY;
        _sizeZ = sizeZ;
    }


    public IEnumerable<Vector3> GetVertices()
    {
        var halfSizeX = _sizeX / 2;
        var halfSizeY = _sizeY / 2;
        var halfSizeZ = _sizeZ / 2;

        var x = _center.X;
        var y = _center.Y;
        var z = _center.Z;

        var vertices = new List<Vector3>
        {
            new(x - halfSizeX, y - halfSizeY, z - halfSizeZ),
            new(x + halfSizeX, y - halfSizeY, z - halfSizeZ),
            new(x + halfSizeX, y + halfSizeY, z - halfSizeZ),
            new(x - halfSizeX, y + halfSizeY, z - halfSizeZ),
            new(x - halfSizeX, y - halfSizeY, z + halfSizeZ),
            new(x + halfSizeX, y - halfSizeY, z + halfSizeZ),
            new(x + halfSizeX, y + halfSizeY, z + halfSizeZ),
            new(x - halfSizeX, y + halfSizeY, z + halfSizeZ)
        };

        return vertices;
    }

    public IEnumerable<Face> GetFaces()
    {
        var vertices = GetVertices().ToList();
        var faces = new List<Face>
        {
            new([1, 2, 3]),
            new([1, 3, 4]),
            new([5, 8, 7]),
            new([5, 7, 6]),
            new([1, 5, 6]),
            new([1, 6, 2]),
            new([2, 6, 7]),
            new([2, 7, 3]),
            new([3, 7, 8]),
            new([3, 8, 4]),
            new([5, 1, 4]),
            new([5, 4, 8])
        };

        return faces;
    }
}

public class ExportHelper
{
    public void ExportToObj(IEnumerable<Voxel> voxels, string path)
    {
        var vertices = new List<Vector3>();
        var faces = new List<Vector3>();

        var sbVerticies = new StringBuilder();
        var sbFaces = new StringBuilder();

        var i = 0;
        foreach (var voxel in voxels)
        {
            foreach (var vertex in voxel.GetVertices())
            {
                sbVerticies.AppendLine(
                    $"v {vertex.X.ToString().Replace(",", ".")} {vertex.Y.ToString().Replace(",", ".")} {vertex.Z.ToString().Replace(",", ".")}");
            }

            var offset = i * 8;
            foreach (var face in voxel.GetFaces())
            {
                sbFaces.AppendLine(
                    $"f {face.Indices[0] + offset} {face.Indices[1] + offset} {face.Indices[2] + offset}");
            }

            i++;
        }

        File.WriteAllText(path, sbVerticies.AppendLine(sbFaces.ToString()).ToString());
    }

    private int GetIndexOfVertex(IReadOnlyList<Vector3> vertices, Vector3 vertex)
    {
        for (var i = 0; i < vertices.Count; i++)
        {
            if (vertices[i] == vertex)
            {
                return i + 1;
            }
        }

        return -1;
    }
}

public class PCExporter
{
    public void ExportToPC(IEnumerable<Vector3> points, string path)
    {
        var sbVerticies = new StringBuilder();

        var i = 0;
        foreach (var point in points)
        {
           
            sbVerticies.AppendLine(
                $"{point.X.ToString().Replace(",", ".")} {point.Y.ToString().Replace(",", ".")} {point.Z.ToString().Replace(",", ".")}");
          
        }

        File.WriteAllText(path, sbVerticies.ToString());
    }
    
}