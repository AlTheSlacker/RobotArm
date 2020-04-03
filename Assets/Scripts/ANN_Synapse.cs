using System.Collections;
using System.Collections.Generic;

public class ANN_Synapse
{
    private double weight;
    private double learningRate;
    private Neuron outputNeuron;
    private Neuron inputNeuron;
    private List<double> deltaInput = new List<double>();

    public double Weight { get => weight; set => weight = value; }
    public double LearningRate { get => learningRate; set => learningRate = value; }
    public Neuron OutputNeuron { get => outputNeuron; set => outputNeuron = value; }
    public Neuron InputNeuron { get => inputNeuron; set => inputNeuron = value; }
    public List<double> DeltaInput { get => deltaInput; }

    public ANN_Synapse()
    {
    }

    public void Activate() => OutputNeuron.AddInput(InputNeuron.Output * Weight);
    public void DeltaInputAdd(double additionalDelta) => deltaInput.Add(additionalDelta);
    public void DeltaInputClear() => deltaInput.Clear();
    public void EpochWeightsProcess() => Weight = Weight - LearningRate * ListAverage();

    private double ListAverage()
    {
        double total = 0;
        for (int i = 0; i < DeltaInput.Count; i++)
        {
            total += DeltaInput[i];
        }
        return total / DeltaInput.Count;
    }
}
