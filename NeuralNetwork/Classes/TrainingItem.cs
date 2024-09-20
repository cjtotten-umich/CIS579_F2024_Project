using System.Drawing;

namespace NeuralNetwork
{
    public class TrainingItem
    {
        public string Name { get; set; }

        public Bitmap Image { get; set; }

        public Volume Truth { get; set; }

        public double LastError { get; set; }

        public TrainingItem(string name, Bitmap image, Volume truth) 
        {
            Name = name;
            Image = image;
            Truth = truth;
            LastError = 0;
        }

    }
}
