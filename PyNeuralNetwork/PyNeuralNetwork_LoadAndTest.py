import os
import numpy as np
import pandas as pd
from tensorflow.keras.models import load_model
from tensorflow.keras.preprocessing.image import load_img, img_to_array

model = load_model('model.keras', compile=False)

IMG_HEIGHT, IMG_WIDTH = 512, 512
image_dir = '/tmp/TestData/' 

def preprocess_image(image_path):
    img = load_img(image_path, target_size=(IMG_HEIGHT, IMG_WIDTH))
    
    img_array = img_to_array(img) / 255.0
    #img_array = img_array[:, :, 0:1] # this will make us only have the red channel
    return np.expand_dims(img_array, axis=0)

def predict_image(image_path):
    img = preprocess_image(image_path)
    prediction = model.predict(img)
    probability_prediction = prediction[0][0] 
    position_prediction = prediction[1][0] 

    return probability_prediction, position_prediction

def predict_directory(image_dir):
    results = []
    i = 0
    for filename in os.listdir(image_dir):
        i += 1
        if filename.endswith('.jpg') and i < 1000:
            image_path = os.path.join(image_dir, filename)
            # Get class and bounding box predictions
            probability_prediction, position_prediction = predict_image(image_path)
            # Append the results (filename, class, bounding box)
            results.append({
                'filename': filename,
                'probability': probability_prediction,
                'x': position_prediction[0],
                'y': position_prediction[1]
            })
    
    return pd.DataFrame(results)

results_df = predict_directory(image_dir)

results_df.to_csv('predictions.csv', index=False)

print(results_df.head())