using UnityEngine;

namespace MusicVideoGenerator
{
    /// <summary>
    /// Types of procedurally generated shapes
    /// </summary>
    public enum ShapeType
    {
        OrbitalSystem,      // Core with satellites
        CrystalCluster,     // Radiating shards
        Tower,              // Stacked segments
        Ring,               // Circle of objects
        StarBurst,          // Central sphere with spikes
        Helix,              // DNA spiral
        Lattice,            // 3D grid
        AbstractSculpture,  // Random arrangement
        SpiralGalaxy,       // Spiral arms
        Molecular,          // Atoms and bonds
        FractalTree,        // Branching structure
        Gyroscope,          // Intersecting rings
        Mandala,            // Concentric pattern
        Atom,               // Nucleus with electron shells
        GeometricFlower,    // Flower with petals
        WireframePolyhedron,// Vertex and edge structure
        Torus,              // Donut shape
        ChainLinks,         // Interlocking rings
        PyramidArray,       // Stepped pyramid
        FibonacciSpiral,    // Golden ratio spiral
        SimplePrimitive     // Single object (background/ambient)
    }
}