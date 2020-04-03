using System;
using System.Collections;
using System.Collections.Generic;

public class ANN_Network
{

    private Neuron[][] netLayers;
    private List<ANN_Synapse> synapsesAll = new List<ANN_Synapse>();

    public ANN_Network(int inputs, int[] hiddenLayers, AFType[] afFunctions, int outputs, bool[] addBias, double learningRate)
    {
        // create array of neurons for each layer
        // create a jagged array of neuron layers: 1 input + hiddenlayers + 1 output
        netLayers = new Neuron[hiddenLayers.Length + 2][];
        netLayers[0] = new Neuron[inputs];
        for (int hiddenLayer = 0; hiddenLayer < hiddenLayers.Length; hiddenLayer++)
        {
            netLayers[hiddenLayer + 1] = new Neuron[hiddenLayers[hiddenLayer]];
        }
        netLayers[hiddenLayers.Length + 1] = new Neuron[outputs];

        // setup the neuron activation functions
        NeuronsInit(afFunctions);

        // create synapses for each neuron in each layer and set up connections
        ANN_Synapse newSynapse;
        System.Random rand = new System.Random();
        for (int currentLayer = 0; currentLayer < netLayers.Length - 1; currentLayer++)
        {
            for (int inpNeuron = 0; inpNeuron < netLayers[currentLayer].Length; inpNeuron++)
            {
                for (int outNeuron = 0; outNeuron < netLayers[currentLayer + 1].Length; outNeuron++)
                {
                    newSynapse = new ANN_Synapse
                    {
                        InputNeuron = netLayers[currentLayer][inpNeuron],
                        OutputNeuron = netLayers[currentLayer + 1][outNeuron],
                        Weight = 1 - rand.NextDouble() * 4, // WEIGHT
                        LearningRate = learningRate
                    };
                    synapsesAll.Add(newSynapse);
                    netLayers[currentLayer][inpNeuron].AddOutputSynapse(newSynapse);
                    netLayers[currentLayer + 1][outNeuron].AddInputSynapse(newSynapse);
                }
            }
        }

        // add bias neurons
        int previousLayer;
        Neuron biasNeuron;
        for (int currentLayer = 1; currentLayer < netLayers.Length; currentLayer++)
        {
            previousLayer = currentLayer - 1;
            if (addBias[currentLayer])
            {
                biasNeuron = new Neuron(AFType.Bias);
                Array.Resize(ref netLayers[previousLayer], netLayers[previousLayer].Length + 1);
                netLayers[previousLayer][netLayers[previousLayer].Length - 1] = biasNeuron;
                biasNeuron.Update();
                for (int outNeuron = 0; outNeuron < netLayers[currentLayer].Length; outNeuron++)
                {
                    newSynapse = new ANN_Synapse
                    {
                        InputNeuron = biasNeuron,
                        OutputNeuron = netLayers[currentLayer][outNeuron],
                        Weight = rand.NextDouble(),
                        LearningRate = learningRate
                    };
                    synapsesAll.Add(newSynapse);
                    biasNeuron.AddOutputSynapse(newSynapse);
                    netLayers[currentLayer][outNeuron].AddInputSynapse(newSynapse);
                }
            }
        }

    }

    public ANN_Synapse SynapseFind(int layer, int inputNeuron, int outputNeuron)
    {
        return synapsesAll.Find(syn => syn.InputNeuron == netLayers[layer][inputNeuron] && syn.OutputNeuron == netLayers[layer + 1][outputNeuron]);
    }

    public void InputsAdd(double[] netInputs)
    {
        for (int neuron = 0; neuron < netInputs.Length; neuron++)
        {
            netLayers[0][neuron].Input = netInputs[neuron];
            netLayers[0][neuron].Update();
        }
    }

    public void FeedForward()
    {
        int nextLayer;

        for (int currentLayer = 0; currentLayer < netLayers.Length; currentLayer++)
        {
            nextLayer = currentLayer + 1;
            NeuronsUpdateByLayer(currentLayer);
            LayerInputReset(nextLayer);
            for (int inpNeuron = 0; inpNeuron < netLayers[currentLayer].Length; inpNeuron++)
            {
                netLayers[currentLayer][inpNeuron].SynapseFeedforwardAll();
            }
        }
        NeuronsUpdateByLayer(netLayers.Length - 1);
    }

    public void BackPropagate(double[][] sampledata, double[][] targetdata, int epochs)
    {
        int datasets = sampledata.Length;
        int outputLayerID = netLayers.Length - 1;
        double derivAF, derivError;
        Neuron neuron;
        ANN_Synapse synapse;

        for (int epoch = 0; epoch < epochs; epoch++)
        {
            // clear any historic data from synapse epoch weights
            for (int synapseID = 0; synapseID < synapsesAll.Count; synapseID++)
            {
                synapsesAll[synapseID].DeltaInputClear();
            }
            for (int dataset = 0; dataset < sampledata.Length; dataset++)
            {
                InputsAdd(sampledata[dataset]);
                FeedForward();
                // calculate output deltas
                for (int outNeuronID = 0; outNeuronID < netLayers[netLayers.Length - 1].Length; outNeuronID++)
                {
                    neuron = netLayers[outputLayerID][outNeuronID];
                    derivAF = neuron.DerivAF(neuron.Input);
                    derivError = -(targetdata[dataset][outNeuronID] - neuron.Output);
                    neuron.BackpropDelta = derivAF * derivError;
                }
                // calculate remaining deltas
                for (int layerID = outputLayerID - 1; layerID > 0; layerID--)
                {
                    for (int neuronID = 0; neuronID < netLayers[layerID].Length; neuronID++)
                    {
                        netLayers[layerID][neuronID].BackPropDeltaUpdate();
                    }
                }
                // add delta * input neuron output to the synapse deltainput list
                for (int synapseID = 0; synapseID < synapsesAll.Count; synapseID++)
                {
                    synapse = synapsesAll[synapseID];
                    synapse.DeltaInputAdd(synapse.OutputNeuron.BackpropDelta * synapse.InputNeuron.Output);
                }
            }
            // update weights
            for (int synapseID = 0; synapseID < synapsesAll.Count; synapseID++)
            {
                synapsesAll[synapseID].EpochWeightsProcess();
            }
        }
    }

    public double[] OutputsGet()
    {
        int iOutputs = netLayers[netLayers.Length - 1].Length;
        double[] outputs = new double[iOutputs];
        for (int iNeuron = 0; iNeuron < iOutputs; iNeuron++)
        {
            outputs[iNeuron] = netLayers[netLayers.Length - 1][iNeuron].Output;
        }
        return outputs;
    }

    private void LayerInputReset(int updateLayer)
    {
        if (updateLayer > netLayers.Length - 1) return;
        for (int neuron = 0; neuron < netLayers[updateLayer].Length; neuron++)
        {
            netLayers[updateLayer][neuron].Input = 0;
        }
    }

    private void NeuronsUpdateAll()
    {
        for (int layer = 0; layer < netLayers.Length; layer++)
        {
            NeuronsUpdateByLayer(layer);
        }
    }

    private void NeuronsUpdateByLayer(int updateLayer)
    {
        for (int neuron = 0; neuron < netLayers[updateLayer].Length; neuron++)
        {
            netLayers[updateLayer][neuron].Update();
        }
    }

    public int SynapseCount()
    {
        return synapsesAll.Count;
    }

    public int SynapseCountByLayer(int layers)
    {
        if(layers >= netLayers.Length)
        {
            return 0;
        }
        return netLayers[layers].Length * netLayers[layers + 1].Length;
    }

    public int LayerCount()
    {
        return netLayers.Length;
    }

    public void WeightSet(int layer, int inpNeuron, int outNeuron, double newWeight)
    {
        ANN_Synapse synapse = SynapseFind(layer, inpNeuron, outNeuron);
        synapse.Weight = newWeight;
    }

    public double WeightGet(int layer, int inpNeuron, int outNeuron)
    {
        ANN_Synapse synapse = SynapseFind(layer, inpNeuron, outNeuron);
        return synapse.Weight;
    }

    public double ErrorTotal(double[] target, double[] actual)
    {
        double totalError = 0;
        for (int i = 0; i < target.Length; i++)
        {
            totalError += ErrorSquared(target[i], actual[i]);
        }
        return totalError;
    }

    private double ErrorSquared(double target, double actual)
    {
        double difference = target - actual;
        return 0.5 * difference * difference;
    }

    private void NeuronsInit(AFType[] afFunctions)
    {
        for (int layer = 0; layer < netLayers.Length; layer++)
        {
            for (int neuron = 0; neuron < netLayers[layer].Length; neuron++)
            {
                netLayers[layer][neuron] = new Neuron(afFunctions[layer]);
            }
        }
    }

}