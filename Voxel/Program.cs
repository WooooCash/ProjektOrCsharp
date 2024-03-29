﻿using System.Numerics;
using Assimp;
using Auios.QuadTree;
using Voxel;

var importer = new AssimpContext();
var skullPath = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Resources", "skull.obj");
var skullScene = importer.ImportFile(skullPath);


var faces = skullScene.Meshes[0].Faces;
var vertices = skullScene.Meshes[0].Vertices;

var minPointX = vertices.Min(vector => vector.X);
var minPointY = vertices.Min(vector => vector.Y);
var minPointZ = vertices.Min(vector => vector.Z);
var maxPointX = vertices.Max(vector => vector.X);
var maxPointY = vertices.Max(vector => vector.Y);
var maxPointZ = vertices.Max(vector => vector.Z);

var origin = new Vector3(minPointX, minPointY, minPointZ);
var maxPoint = new Vector3(maxPointX, maxPointY, maxPointZ);

var quadTree = new QuadTree<Face>(origin.X, origin.Y, maxPointX - origin.X, maxPointY - origin.Y, new ObjectBound(vertices));
quadTree.InsertRange(faces);

var dims = new[] { 256, 256, 256 };
var deltaX = (maxPointX - minPointX)/dims[0];
var deltaY = (maxPointY - minPointY)/dims[1];
var deltaZ = (maxPointZ - minPointZ)/dims[2];

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
        
        var rect = new QuadTreeRect(point.X, point.Y, deltaX, deltaY);
        var foundFaces = quadTree.FindObjects(rect);
        
        foreach (var face in foundFaces)
        {
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
            var count = zList.Count(zValue => zValue > zc + z * deltaZ);

            if (count % 2 == 1)
            {
                resultCenterPoints.Add(new Vector3(x, y, zc));
            }

            zc += deltaZ;
        }
    }
}

var exporter = new ExportHelper();
var voxels = resultCenterPoints.Select(vector => new Voxel.Voxel(vector, deltaX, deltaY, deltaZ)).ToList();
exporter.ExportToObj(voxels, Path.Join(AppDomain.CurrentDomain.BaseDirectory, "Resources", "result.obj"));

Console.WriteLine(resultCenterPoints.Count);