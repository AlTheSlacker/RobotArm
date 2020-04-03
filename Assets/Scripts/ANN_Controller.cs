using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ANN_Controller : MonoBehaviour
{
    // used to initially train the network - may take several hours to run
    [SerializeField] private bool train = true;
    [SerializeField] private int inputs = 2;
    [SerializeField] private int[] hiddenLayers = new int[] { 3, 3 };
    [SerializeField] private bool[] addBias = new bool[] { false, true, true, false };
    [SerializeField] private AFType[] afFunctions = new AFType[] { AFType.Base, AFType.LeakyReLu, AFType.Sigmoid, AFType.Base };
    [SerializeField] private int outputs = 3;
    [SerializeField] private double learningRate = 0.05;
    [SerializeField] private int epochs = 20000;




    private ANN_Network net;
    private double[][] sampledata = new double[6859][];
    private double[][] targetdata = new double[6859][];




    void Start()
    {
        // create the network
        net = new ANN_Network(inputs, hiddenLayers, afFunctions, outputs, addBias, learningRate);

        if (train)
        {
            TrainNetwork();
            SaveWeights();
        }
        else
        {
            LoadWeights();
        }


        // debug crap to be deleted **********************************************

        // define specific input and run trained network on that input
        double[] inputValues = new double[] { -0.9, 0.55 };
        double[] outputValues = FeedForward(inputValues);
        double armLength1 = 0.4584155;
        double armLength2 = 0.5481802;
        double armLength3 = 0.1586066;
        print("Final output Theta1: " + outputValues[0]);
        print("Final output Theta2: " + outputValues[1]);
        print("Final output Theta3: " + outputValues[2]);
        print("Actual X: " + armLength1 * Math.Cos(Math.PI / 180 * outputValues[0]) + armLength2 * Math.Cos(Math.PI / 180 * (outputValues[0] + outputValues[1])) + armLength3 * Math.Cos(Math.PI / 180 * (outputValues[0] + outputValues[1] + outputValues[2])));
        print("Target X: -0.90");
        print("Actual Y: " + armLength1 * Math.Sin(Math.PI / 180 * outputValues[0]) + armLength2 * Math.Sin(Math.PI / 180 * (outputValues[0] + outputValues[1])) + armLength3 * Math.Sin(Math.PI / 180 * (outputValues[0] + outputValues[1] + outputValues[2])));
        print("Target Y: 0.55");

        // debug crap to be deleted **********************************************


        print("Finished");
    }

    private void CreateData()
    {
        // this is the training data, exclusively used to train the network and bespoke to the current problem
        // anything can happen here as long as valid data is generated for sampledata and targetdata
        // where sample is the input and target is the output
        // once the netrwork is trained it will predict the output based on the input data

        int sample = 0;
        double armLength1 = 0.4584155;
        double armLength2 = 0.5481802;
        double armLength3 = 0.1586066;

        for (int theta1 = 0; theta1 <= 180; theta1 += 10)
        {
            for (int theta2 = 0; theta2 <= 180; theta2 += 10)
            {
                for (int theta3 = 0; theta3 <= 180; theta3 += 10)
                {
                    sampledata[sample] = new double[2] {
                        armLength1 * Math.Cos(Math.PI / 180 * theta1) + armLength2 * Math.Cos(Math.PI / 180 * (theta1 + theta2)) + armLength3 * Math.Cos(Math.PI / 180 * (theta1 + theta2 + theta3)),
                        armLength1 * Math.Sin(Math.PI / 180 * theta1) + armLength2 * Math.Sin(Math.PI / 180 * (theta1 + theta2)) + armLength3 * Math.Sin(Math.PI / 180 * (theta1 + theta2 + theta3))
                    };
                    targetdata[sample] = new double[3] { theta1, theta2, theta3 };
                    sample++;
                }
            }
        }
        return;
    }

    private void TrainNetwork()
    {
        // create sample and target training data
        CreateData();

        // train network
        net.BackPropagate(sampledata, targetdata, epochs);

    }

    private void SaveWeights()
    {
        // save weights
        // -----------------------------------------------------------

        /*
        double[][] weights = new double[net.LayerCount()][];
        for (int i = 0; i < net.LayerCount(); i++)
        {
            weights[i] = new double
        }
        */

        // -----------------------------------------------------------
    }

    private void LoadWeights()
    {
        // load previously saved weights data




    }

    private double[] FeedForward(double[] inputValues)
    {
        net.InputsAdd(inputValues);
        net.FeedForward();
        return net.OutputsGet();
    }

}
