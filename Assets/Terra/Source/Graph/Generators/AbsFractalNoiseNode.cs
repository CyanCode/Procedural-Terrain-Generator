namespace Terra.Graph.Generators {
	public abstract class AbsFractalNoiseNode: AbsGeneratorNode {
		[Input] public float Frequency = 1f;
		[Input] public float Lacunarity = 2.17f;
		[Input] public int OctaveCount = 6;
	}
}