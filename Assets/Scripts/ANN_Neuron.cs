using System;
using System.Collections;
using System.Collections.Generic;


public enum AFType
{
    Base = 0,
    LeakyReLu = 1,
    Sigmoid = 2,
    Bias = 100
}


public class Neuron
{
    private double input;
    private double output;
    private double backpropDelta;
    private AFType activationFunction;
    private List<ANN_Synapse> outputSynapses = new List<ANN_Synapse>();
    private List<ANN_Synapse> inputSynapses = new List<ANN_Synapse>();


    public double Input { get => input; set => input = value; }
    public double Output { get => output; }
    public double BackpropDelta { get => backpropDelta; set => backpropDelta = value; }
    public AFType ActivationFunction { get => activationFunction; }
    public List<ANN_Synapse> OutputSynapses { get => outputSynapses; }
    public List<ANN_Synapse> InputSynapses { get => inputSynapses; }

    public Neuron(AFType aF) => activationFunction = aF;
    public void AddOutputSynapse(ANN_Synapse synapse) => outputSynapses.Add(synapse);
    public void AddInputSynapse(ANN_Synapse synapse) => inputSynapses.Add(synapse);
    public void AddInput(double additionalInput) => Input += additionalInput;
    public void Update() => output = AF(input);

    public void SynapseFeedforwardAll()
    {
        for (int synapseIndex = 0; synapseIndex < outputSynapses.Count; synapseIndex++)
        {
            SynapseFeedforward(outputSynapses[synapseIndex]);
        }
    }

    public void SynapseFeedforward(ANN_Synapse synapse) => synapse.Activate();

    public void BackPropDeltaUpdate()
    {
        double delta = 0;
        double derivAF = DerivAF(Input);
        for (int synapseID = 0; synapseID < outputSynapses.Count; synapseID++)
        {
            delta += derivAF * outputSynapses[synapseID].Weight * outputSynapses[synapseID].OutputNeuron.backpropDelta;
        }
        backpropDelta = delta;
    }

    public void Status()
    {
        Console.WriteLine("Input: " + Input + " Output: " + Output);
    }

    public void PrintWeights()
    {
        for (int synapseIndex = 0; synapseIndex < outputSynapses.Count; synapseIndex++)
        {
            Console.WriteLine(outputSynapses[synapseIndex].Weight);
        }
    }

    private double AF(double x)
    {
        switch ((int)ActivationFunction)
        {
            case 0:
                return AFBase(x);
            case 1:
                return AFLeakyReLu(x);
            case 2:
                return AFSigmoid(x);

            case 100:
                return 1;  // bias node
        }
        return 0;
    }

    private double AFBase(double x)
    {
        return x;
    }

    private double AFLeakyReLu(double x)
    {
        return Math.Max(0, x);
    }

    private double AFSigmoid(double x)
    {
        if (x < -45.0) return 0.0;
        else if (x > 45.0) return 1.0;
        else return 1.0 / (1.0 + Math.Exp(-x));
    }

    public double DerivAF(double x)
    {
        switch ((int)ActivationFunction)
        {
            case 0:
                return DerivAFBase(x);
            case 1:
                return DerivAFLeakyReLu(x);
            case 2:
                return DerivAFSigmoid(x);

            case 100:
                return 1; // bias node
        }
        return 0;
    }

    private double DerivAFBase(double x)
    {
        return 1;
    }

    private double DerivAFLeakyReLu(double x)
    {
        return Math.Max(0, 1);
    }

    private double DerivAFSigmoid(double x)
    {
        double aFSig = AFSigmoid(x);
        return aFSig * (1 - aFSig);
    }

}
