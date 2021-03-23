using System;
using System.Collections.Generic;
using Google.Maps.Feature;
using Google.Maps.Feature.Shape;
using GUTGuide.Patterns;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Code.Styling
{
    /// <summary>
    /// Responsible for creating extrusions from given geometry
    /// </summary>
    public class ExtrusionModifier : PersistentSingleton<ExtrusionModifier>
    {
        [Header("General settings")] 
        
        [Tooltip("Default thickness of created extrusions")] 
        [SerializeField] 
        [Min(0.1f)]
        private float defaultThickness = 1;
        
        [Tooltip("Default height applied to Maps SDK for Unity generated buildings that do not have stored height in " +
                 "formation. The chosen value of 10f matches the default value used inside the Maps SDK for " +
                 "buildings without stored heights")]
        [SerializeField] 
        private float defaultBuildingHeight = 10;

        [Header("Naming")] 
        
        [Tooltip("Name given to GameObjects created as building base decorations")] 
        [SerializeField]
        private string borderName = "Border";

        [Tooltip("Name given to GameObjects created as parapets")] 
        [SerializeField]
        private string parapetName = "Parapet";

        /// <summary>
        /// Two dimensional cross-sections we will use to form flat extrusions around a given shape
        /// </summary>
        private Vector2[] BorderShape => new[] {Vector2.right, Vector2.zero};

        /// <summary>
        /// Two dimensional cross-sections that will be used to form the parapets
        /// </summary>
        private Vector2[][] ParapetShapes => new[]
        {
            // A square parapet running along the outer edge of a roof, not overhanging exterior walls.
            ConvertFloatsToVector2Array(0f, 0f, 0f, 1f, -1f, 1f, -1f, 0f),

            // A square parapet running along the outer edge of a roof, slightly overlapping the roof, and
            // overhanging exterior walls.
            ConvertFloatsToVector2Array(-0.5f, 0f, 1f, 0f, 1f, 1f, -0.5f, 1f, -0.5f, 0f),

            // A stepped parapet that overhangs exterior walls, with the steps facing down towards the
            // ground.
            ConvertFloatsToVector2Array(-1f, 0f, 0.5f, 0f, 0.5f, 0.5f, 1f, 0.5f, 1f, 1.0f, -1f, 1f, -1f, 0f),

            // A stepped parapet that does not overhang exterior walls, with the steps facing upwards
            // towards the sky.
            ConvertFloatsToVector2Array(0f, 0f, 0f, 1f, -0.5f, 1f, -0.5f, 0.5f, -1f, 0.5f, -1f, 0f),

            // A bevelled parapet that overhangs exterior walls, similar to the steps facing upwards but
            // with a slope in place of the middle step.
            ConvertFloatsToVector2Array(0f, -0.5f, 1f, -0.5f, 1f, 0.5f, 0.5f, 1f, 0f, 1f, 0f, -0.5f)
        };

        /// <summary>
        /// Adds a extruded border around the base of a given building
        /// </summary>
        /// <param name="buildingGameObject">The Maps SDK for Unity created <see cref="GameObject"/> containing this
        /// building</param>
        /// <param name="buildingShape">The Maps SDK for Unity <see cref="MapFeature"/> data defining this building's
        /// shape and height</param>
        /// <param name="borderMaterial">The <see cref="Material"/> to apply to the extrusion once it is created</param>
        /// <param name="thickness">Thickness of extrusion</param>
        /// <param name="yOffset">Amount to raise created shape vertically</param>
        /// <returns></returns>
        public List<GameObject> AddBuildingBorder(GameObject buildingGameObject, ExtrudedArea buildingShape,
            Material borderMaterial, float? thickness = null, float? yOffset = null)
        {
            // Create list to store all created borders
            var extrudedBorders = new List<GameObject>();
            var resultThickness = thickness ?? defaultThickness;
            var resultYOffset = yOffset ?? 0;

            // Use general-purpose building-extrusion function to add border around building
            foreach (var extrusion in buildingShape.Extrusions)
            {
                var extrusions = AddExtrusionToBuilding(buildingGameObject, borderMaterial, extrusion, BorderShape,
                    resultYOffset, resultThickness);
                extrudedBorders.AddRange(extrusions);
            }

            return extrudedBorders;
        }

        /// <summary>
        /// Adds a parapet of a randomly chosen cross-section to the given building
        /// </summary>
        /// <param name="buildingGameObject">The Maps SDK for Unity created <see cref="GameObject"/> containing this
        /// building</param>
        /// <param name="buildingShape">The Maps SDK for Unity <see cref="MapFeature"/> data defining this building's
        /// shape and height</param>
        /// <param name="parapetMaterial">The <see cref="Material"/> to apply to the parapet once it is created</param>
        /// <param name="parapetType">Optional index of parapet to cross-section to use. Will use a randomly chosen
        /// cross-section if no index given, or if given index is invalid</param>
        /// <returns></returns>
        public List<GameObject> AddBuildingParapet(GameObject buildingGameObject, ExtrudedArea buildingShape,
            Material parapetMaterial, int? parapetType = null)
        {
            // Create list to store all created parapets
            var extrudedParapets = new List<GameObject>();

            foreach (var extrusion in buildingShape.Extrusions)
            {
                var height = extrusion.MaxZ > 0.1f
                    ? extrusion.MaxZ
                    : defaultBuildingHeight;

                var resultParapetType = parapetType >= 0 && parapetType.Value < ParapetShapes.Length
                    ? parapetType.Value
                    : Random.Range(0, ParapetShapes.Length);

                var extrusions = AddExtrusionToBuilding(buildingGameObject, parapetMaterial,
                    extrusion, ParapetShapes[resultParapetType], height, defaultThickness, true);
                extrudedParapets.AddRange(extrusions);
            }

            return extrudedParapets;
        }
        
        /// <summary>
        /// Adds a extruded shape for a given <see cref="ExtrudedArea.Extrusion"/> of a given building
        /// </summary>
        /// <param name="buildingGameObject">The Maps SDK for Unity created <see cref="GameObject"/> containing this
        /// building</param>
        /// <param name="extrusionMaterial">The <see cref="Material"/> to apply to the extrusion once it is created</param>
        /// <param name="extrusion">Current <see cref="ExtrudedArea.Extrusion"/> to extrude in given building</param>
        /// <param name="crossSection">The 2D crossSection of the shape to loft along the path</param>
        /// <param name="yOffset">Amount to raise created shape vertically</param>
        /// <param name="thickness">Thickness of extrusion</param>
        /// <param name="isParapet">Whether or not desired extrusion is a parapet</param>
        /// <returns></returns>
        private IEnumerable<GameObject> AddExtrusionToBuilding(GameObject buildingGameObject, Material extrusionMaterial,
            ExtrudedArea.Extrusion extrusion, IList<Vector2> crossSection, float yOffset, float thickness,
            bool isParapet = false)
        {
            // Build resultant extrusions list
            var extrusionGameObjects = new List<GameObject>();
            // Build an extrusion in local space
            var loops = PadEdgeSequences(extrusion.FootPrint.GenerateBoundaryEdges());

            foreach (var sequence in loops)
            {
                // Try to make extrusion
                var extrusionGameObjectName = isParapet ? parapetName : borderName;
                var extrusionGameObject = GenerateExtrusionGameObject(extrusionGameObjectName, extrusionMaterial,
                    sequence.Vertices, crossSection, thickness);

                if (extrusionGameObject)
                {
                    // Parent extrusion to the building object
                    extrusionGameObject.transform.SetParent(buildingGameObject.transform);
                    
                    // Align created extrusion to the building world position
                    extrusionGameObject.transform.localPosition = Vector3.up * yOffset;
                    
                    // Add to extrusion resultant list
                    extrusionGameObjects.Add(extrusionGameObject);
                }
                else
                {
                    Debug.LogError("Not able to create extrusion for a building!");
                }
            }

            return extrusionGameObjects;
        }

        /// <summary>
        /// Convert a given array of floats into an array of <see cref="Vector2"/>s
        /// </summary>
        /// <param name="numbers">An array of floats to be converted to Vector2[]</param>
        /// <returns>An array of Vector2 made from given floats</returns>
        /// <exception cref="ArgumentException">Thrown when the length of given parameters are not even</exception>
        private Vector2[] ConvertFloatsToVector2Array(params float[] numbers)
        {
            // Confirm an even number of floats have been given
            if (numbers.Length % 2 != 0)
            {
                throw new ArgumentException("Arguments must be provided in pairs", nameof(numbers));
            }

            //  Return each pair of floats as one element of an array of Vector2's
            var vectors = new Vector2[numbers.Length / 2];

            for (var index = 0; index < numbers.Length; index += 2)
            {
                vectors[index / 2] = new Vector2(numbers[index], numbers[index + 1]);
            }

            return vectors;
        }
        
        /// <summary>
        /// Create extrusion geometry using the supplied path
        /// </summary>
        /// <param name="gameObjectName">The name that should be given to the created game object</param>
        /// <param name="extrusionMaterial"><see cref="Material"/> to apply to created extrusion</param>
        /// <param name="path">The 2D corners of the building footprint</param>
        /// <param name="crossSection">The 2D crossSection of the shape to loft along the path</param>
        /// <param name="thickness">Thickness of loft</param>
        /// <returns></returns>
        private GameObject GenerateExtrusionGameObject(string gameObjectName, Material extrusionMaterial,
            IList<Vector2> path, IList<Vector2> crossSection, float thickness)
        {
            // Prepare data containers

            if (!TryExtrude(path, crossSection, thickness, out var vertices, out var trianglesIndices,
                out var uvs)) return null;
            
            // Prepare game object
            var extrusionGameObject = new GameObject(gameObjectName);
            var extrusionMeshFilter = extrusionGameObject.AddComponent<MeshFilter>();
            var extrusionMeshRenderer = extrusionGameObject.AddComponent<MeshRenderer>();

            // Add a mesh cleaner to prevent memory leak caused by dynamically generated meshes
            extrusionGameObject.AddComponent<MeshCleaner>();
                
            // Set the material
            extrusionMeshRenderer.material = extrusionMaterial;
                
            // Create mesh from obtained values
            var mesh = new Mesh()
            {
                vertices = vertices,
                triangles = trianglesIndices,
                uv = uvs
            };
            mesh.RecalculateNormals();
                
            // Apply mesh
            extrusionMeshFilter.mesh = mesh;

            return extrusionGameObject;
        }

        /// <summary>
        /// Returns a canonical representation of the supplied Edge Sequences to facilitate easy creation
        /// </summary>
        /// <param name="edgeSequences">The edge sequences to canonicalize</param>
        /// <returns>Padded copies of supplied edge sequences</returns>
        private List<Area.EdgeSequence> PadEdgeSequences(IReadOnlyCollection<Area.EdgeSequence> edgeSequences)
        {
            var paddedEdgeSequences = new List<Area.EdgeSequence>(edgeSequences.Count);

            foreach (var sequence in edgeSequences)
            {
                // Filter out any wrong sequences
                if (sequence.Vertices.Count < 2) continue;

                // Collect sequence vertices
                var vertices = new List<Vector2>();
                vertices.AddRange(sequence.Vertices);

                var vertexAmount = vertices.Count;
                Vector2 start;
                Vector2 end;

                if (vertexAmount > 2 && vertices[0] == vertices[vertexAmount - 1])
                {
                    start = vertices[vertexAmount - 2];
                    end = vertices[1];
                }
                else
                {
                    start = vertices[0] - (vertices[1] - vertices[0]).normalized;
                    end = vertices[vertexAmount - 1] +
                          (vertices[vertexAmount - 1] - vertices[vertexAmount - 2]).normalized;
                }
                
                vertices.Insert(0, start);
                vertices.Add(end);
                
                paddedEdgeSequences.Add(new Area.EdgeSequence(vertices));
            }

            return paddedEdgeSequences;
        }

        /// <summary>A version of mod that works for negative values.</summary>
        /// <remarks>
        /// This function ensures that returned modulated value will always be positive for values
        /// greater than the negative of the modulus argument.
        /// </remarks>
        /// <param name="modulo">The modulus argument.</param>
        /// <param name="value">The value to modulate.</param>
        /// <returns>A range safe version of value % mod.</returns>
        private int SafeModulo(int modulo, int value)
        {
            return (value + modulo) % modulo;
        }
        
        /// <summary>
        /// Creates a 3d "loft" of a shape along a path by running a given crossSection along the path
        /// </summary>
        /// <param name="path">A padded version of the path along which to loft the given cross-section</param>
        /// <param name="crossSection">The 2D cross-section of the shape to loft along the given path</param>
        /// <param name="thickness">Thickness of loft</param>
        /// <param name="vertices">Outputted mesh vertices</param>
        /// <param name="trianglesIndices">Outputted mesh triangle indices</param>
        /// <param name="uvs">Outputted mesh UVs</param>
        /// <returns>Whether or not lofting succeeded</returns>
        private bool TryExtrude(IList<Vector2> path, IList<Vector2> crossSection, float thickness,
            out Vector3[] vertices, out int[] trianglesIndices, out Vector2[] uvs)
        {
            // Make sure there are enough vertices to extrude
            if (path.Count < 2 || crossSection.Count < 2)
            {
                vertices = null;
                trianglesIndices = null;
                uvs = null;

                return false;
            }

            // Determine the total number of vertices and triangles needed to create the lofted volume
            var segments = path.Count - 3;
            var trianglesIndicesAmount = (crossSection.Count - 1) * segments * 6;
            var trianglesPerSegment = crossSection.Count * 2 - 2;
            var verticesPerJunction = crossSection.Count * 2 - 2;
            var verticesAmount = verticesPerJunction * (segments + 1);

            // Create arrays to hold vertices, uvs and triangle-indices that will be used to create the lofted volume
            vertices = new Vector3[verticesAmount];
            uvs = new Vector2[verticesAmount];
            trianglesIndices = new int[trianglesIndicesAmount];

            // Initialize extruding
            var vertexIndex = 0;
            var triangleIndex = 0;
            const int startCorner = 1;

            // Perform extruding
            for (var cornerIndex = startCorner; cornerIndex < path.Count - 1; cornerIndex++)
            {
                // Make vertices from path corners
                var currentCorner = new Vector3(path[cornerIndex].x, 0, path[cornerIndex].y);
                var nextCorner = new Vector3(path[cornerIndex + 1].x, 0, path[cornerIndex + 1].y);
                var previousCorner = new Vector3(path[cornerIndex - 1].x, 0, path[cornerIndex - 1].y);

                // Get the directions
                var directionToNext = (nextCorner - currentCorner).normalized;
                var directionToPrevious = (previousCorner - currentCorner).normalized;

                // Check if the path direction is counterclockwise
                var turnCrossProduct = Vector3.Cross(directionToNext, directionToPrevious);
                var isCounterclockwise = turnCrossProduct.y < 0;

                // Get the bisector
                var bisector = directionToNext + directionToPrevious;

                // Consider lines to be collinear
                if (bisector.magnitude > 0.015f)
                {
                    bisector.Normalize();
                }
                else
                {
                    bisector = Vector3.Cross(directionToNext, Vector3.down);
                    isCounterclockwise = false;
                }

                var rightBisector = isCounterclockwise ? -bisector : bisector;
                var normalizedExtrusionVector = Vector3.Cross(directionToPrevious, Vector3.up).normalized;
                var distanceToExtrude = thickness / Vector3.Dot(normalizedExtrusionVector, rightBisector);

                // Make sure this extrusion distance is not unrealistically long
                distanceToExtrude = Mathf.Clamp(distanceToExtrude, 0, 2f * thickness);

                // Create a copy of the cross-section shape
                for (var pointInCrossSectionIndex = 0;
                    pointInCrossSectionIndex < crossSection.Count;
                    pointInCrossSectionIndex++)
                {
                    // Align the values of this cross-section point to this corner
                    var pv = crossSection[pointInCrossSectionIndex].x * rightBisector * distanceToExtrude;
                    var uv = crossSection[pointInCrossSectionIndex].y * Vector3.up;
                    var vertex = currentCorner + pv + uv;

                    vertices[vertexIndex] = vertex;
                    uvs[vertexIndex] = new Vector3(vertex.x + vertex.y, vertex.z + vertex.y);

                    // Generate triangle-indices
                    if (pointInCrossSectionIndex > 0 && cornerIndex > startCorner)
                    {
                        trianglesIndices[triangleIndex++] = SafeModulo(verticesAmount, vertexIndex - 1);
                        trianglesIndices[triangleIndex++] =
                            SafeModulo(verticesAmount, vertexIndex - trianglesPerSegment - 1);
                        trianglesIndices[triangleIndex++] = SafeModulo(verticesAmount, vertexIndex);
                        trianglesIndices[triangleIndex++] = SafeModulo(verticesAmount, vertexIndex);
                        trianglesIndices[triangleIndex++] =
                            SafeModulo(verticesAmount, vertexIndex - trianglesPerSegment - 1);
                        trianglesIndices[triangleIndex++] =
                            SafeModulo(verticesAmount, vertexIndex - trianglesPerSegment);
                    }

                    // Copy vertices so can have un-smoothed normals
                    if (pointInCrossSectionIndex > 0 && pointInCrossSectionIndex < crossSection.Count - 1)
                    {
                        vertices[vertexIndex + 1] = vertices[vertexIndex];
                        uvs[vertexIndex + 1] = uvs[vertexIndex];
                        vertexIndex++;
                    }

                    vertexIndex++;
                }
            }

            return true;
        }
    }
}