import os
import numpy as np
import pandas as pd
from tensorflow.keras.models import load_model
from tensorflow.keras.preprocessing.image import load_img, img_to_array

# Load the trained model
model = load_model('model.keras', compile=False)

# Set image size and directories
IMG_HEIGHT, IMG_WIDTH = 512, 512
image_dir = 'c:\\ECE579\\images'  # Directory with test images

# Function to preprocess a single image
def preprocess_image(image_path):
    # Load image with target size
    img = load_img(image_path, target_size=(IMG_HEIGHT, IMG_WIDTH))
    # Convert to array and normalize to [0, 1]
    img_array = img_to_array(img) / 255.0
    # Add batch dimension (1, 512, 512, 3) for prediction
    return np.expand_dims(img_array, axis=0)

# Function to predict on a single image
def predict_image(image_path):
    img = preprocess_image(image_path)
    class_prediction = model.predict(img)
    class_pred = class_prediction[0][0] 

    return class_pred

# Function to predict on all images in a directory
def predict_directory(image_dir):
    # List to store results
    results = []
    max = 1000
    counter = 0
    # Loop through all images in the directory
    for filename in os.listdir(image_dir):
        if filename.endswith('.jpg') and counter < max:
            counter += 1
            image_path = os.path.join(image_dir, filename)
            # Get class and bounding box predictions
            class_pred = predict_image(image_path)
            # Append the results (filename, class, bounding box)
            results.append({
                'filename': filename,
                'class': class_pred
            })
    
    # Convert results to a DataFrame for easier viewing
    return pd.DataFrame(results)

# Predict on the directory and get results
results_df = predict_directory(image_dir)

# Save the results to a CSV file for future reference
results_df.to_csv('predictions.csv', index=False)

# Print the first few results
print(results_df.head())