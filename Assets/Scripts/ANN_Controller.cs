using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ANN_Controller : MonoBehaviour
{
    [SerializeField] private bool train = true;
    [SerializeField] private int inputs = 2;
    [SerializeField] int[] hiddenLayers = new int[] { 3, 3 };
    [SerializeField] bool[] addBias = new bool[] { false, true, true, false };
    [SerializeField] AFType[] afFunctions = new AFType[] { AFType.Base, AFType.LeakyReLu, AFType.Sigmoid, AFType.Base };
    [SerializeField] int outputs = 3;
    [SerializeField] double learningRate = 0.05;
    [SerializeField] int epochs = 20000;

    private ANN_Network net;
    private double[][] sampledata = new double[6859][];
    private double[][] targetdata = new double[6859][];


    // hard coded arm lengths for robot, lazy....
    private double armLength1 = 0.4584155;
    private double armLength2 = 0.5481802;
    private double armLength3 = 0.1586066;

    void Start()
    {
        if (train) CreateAndTrainNetwork();


    }

    private void CreateData()
    {
        int sample = 0;
        
        for (int theta1 = 0; theta1 <= 180; theta1 += 10)
        {
            for (int theta2 = 0; theta2 <= 180; theta2 += 10)
            {
                for (int theta3 = 0; theta3 <= 180; theta3 += 10)
                {
                    sampledata[sample] = new double[2] { X(theta1, theta2, theta3), Y(theta1, theta2, theta3) };
                    targetdata[sample] = new double[3] { theta1, theta2, theta3 };
                    sample++;
                }
            }
        }
        return;
    }

    private double X(double theta1, double theta2, double theta3)
    {
        return armLength1 * Math.Cos(ConvertToRadians(theta1)) + armLength2 * Math.Cos(ConvertToRadians(theta1 + theta2)) + armLength3 * Math.Cos(ConvertToRadians(theta1 + theta2 + theta3));
    }

    private double Y(double theta1, double theta2, double theta3)
    {
        return armLength1 * Math.Sin(ConvertToRadians(theta1)) + armLength2 * Math.Sin(ConvertToRadians(theta1 + theta2)) + armLength3 * Math.Sin(ConvertToRadians(theta1 + theta2 + theta3));
    }

    private double ConvertToRadians(double angle)
    {
        return (Math.PI / 180) * angle;
    }
    
    public void CreateAndTrainNetwork()
    {
        // create the network
        net = new ANN_Network(inputs, hiddenLayers, afFunctions, outputs, addBias, learningRate);

        // create sample and target training data
        CreateData();

        // train network
        net.BackPropagate(sampledata, targetdata, epochs);

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

        // define specific input and run trained network on that input
        double[] inputValues = new double[] { -0.9, 0.55 };
        double[] outputValues = FeedForward(inputValues);
        print("Final output Theta1: " + outputValues[0]);
        print("Final output Theta2: " + outputValues[1]);
        print("Final output Theta3: " + outputValues[2]);
        print("Actual X: " + X(outputValues[0], outputValues[1], outputValues[2]));
        print("Target X: -0.90");
        print("Actual Y: " + Y(outputValues[0], outputValues[1], outputValues[2]));
        print("Target Y: 0.55");
        print("Finished");
    }

    public double[] FeedForward(double[] inputValues)
    {
        net.InputsAdd(inputValues);
        net.FeedForward();
        return net.OutputsGet();
    }

}
