import os
import numpy as np
import pandas as pd
import tensorflow as tf
from tensorflow.keras.models import Model
from tensorflow.keras.layers import Conv2D, MaxPooling2D, Flatten, Dense, Dropout, BatchNormalization, Input
from tensorflow.keras.losses import BinaryCrossentropy, MeanSquaredError
from tensorflow.keras.preprocessing.image import load_img, img_to_array
from tensorflow.keras.optimizers import Adam
from tensorflow.keras import backend as K
from tensorflow.keras import mixed_precision
import warnings
import logging

logging.getLogger('tensorflow').setLevel(logging.ERROR)
logging.getLogger('tensorflow.core').setLevel(logging.ERROR)
warnings.filterwarnings("ignore", category=UserWarning)
warnings.simplefilter("ignore")
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '3'

mixed_precision.set_global_policy('mixed_float16')

IMG_HEIGHT, IMG_WIDTH = 512, 512

def data_generator(image_dir, annotations_file, batch_size, image_size):
    annotations = pd.read_csv(annotations_file)
    num_samples = len(annotations)
    
    while True:
        # Shuffle the annotations at the start of each epoch
        annotations = annotations.sample(frac=1).reset_index(drop=True)

        for offset in range(0, num_samples, batch_size):
            batch_samples = annotations.iloc[offset:offset + batch_size]
            images = []
            probability_labels = []
            position_labels = []
            
            for _, row in batch_samples.iterrows():
                img_path = os.path.join(image_dir, row['filename'])
                img = load_img(img_path, target_size=image_size)
                img = img_to_array(img) / 255.0
                # img = img[:, :, 0:1] # only red channel
                images.append(img)
                
                probability_labels.append(float(row['sign']))
                position_labels.append([row['x'], row['y']])
                
            
            X = np.array(images)
            y_probability = np.array(probability_labels)
            y_probability = np.reshape(y_probability, (-1, 1))
            
            y_position = np.array(position_labels)
            
            yield X, {'probability': y_probability, 'position': y_position}
  
    
print ("Which training set?")
print ("(1) Full Training Set")
print ("(2) Clean Training Set")
print ("(3) Strictly Clean Training Set")
print ("(4) Strictly Clean AND Balanced Training Set")
print ("(5) Small AND Balanced Training Set (50 + 50)")
choice = input("PLEASE ENTER A NUMBER:") 
trainingSetChoice = int(choice)

if trainingSetChoice == 1:
    train_image_dir = '/data/TrainingData/full/images'
    train_annotations_file = '/data/TrainingData/full/annotations.csv'
    val_image_dir = '/data/TrainingData/full/images'
    val_annotations_file = '/data/TrainingData/full/annotations.csv'
elif trainingSetChoice == 2:
    train_image_dir = '/data/TrainingData/clean/images'
    train_annotations_file = '/data/TrainingData/clean/annotations.csv'
    val_image_dir = '/data/TrainingData/clean/images'
    val_annotations_file = '/data/TrainingData/clean/annotations.csv'
elif trainingSetChoice == 3:
    train_image_dir = '/data/TrainingData/strict/images'
    train_annotations_file = '/data/TrainingData/strict/annotations.csv'
    val_image_dir = '/data/TrainingData/strict/images'
    val_annotations_file = '/data/TrainingData/strict/annotations.csv'
elif trainingSetChoice == 4:
    train_image_dir = '/data/TrainingData/strictbalanced/images'
    train_annotations_file = '/data/TrainingData/strictbalanced/annotations.csv'
    val_image_dir = '/data/TrainingData/strictbalanced/images'
    val_annotations_file = '/data/TrainingData/strictbalanced/annotations.csv'
elif trainingSetChoice == 5:
    train_image_dir = '/data/TrainingData/small/images'
    train_annotations_file = '/data/TrainingData/small/annotations.csv'
    val_image_dir = '/data/TrainingData/small/images'
    val_annotations_file = '/data/TrainingData/small/annotations.csv'
else:
    print ('I DO NOT KNOW WHAT YOU WANT')
    exit()


input_layer = Input(shape=(IMG_HEIGHT, IMG_WIDTH, 3)) # change to 1 for only red channel

x = Conv2D(32, (3, 3), activation='relu')(input_layer)
x = BatchNormalization()(x)
x = MaxPooling2D(pool_size=(2, 2))(x)

x = Conv2D(64, (3, 3), activation='relu')(x)
x = BatchNormalization()(x)
x = MaxPooling2D(pool_size=(2, 2))(x)

x = Conv2D(128, (3, 3), activation='relu')(x)
x = BatchNormalization()(x)
x = MaxPooling2D(pool_size=(2, 2))(x)

x = Conv2D(256, (3, 3), activation='relu')(x)
x = BatchNormalization()(x)
x = MaxPooling2D(pool_size=(2, 2))(x)

x = Flatten()(x)
x = Dense(512, activation='relu')(x)
x = Dropout(0.5)(x)

probability_output = Dense(1, activation='sigmoid', name='probability')(x)

position_output = Dense(2, activation='sigmoid', name='position')(x)

model = Model(inputs=input_layer, outputs=[probability_output, position_output])

model.compile(
    optimizer=Adam(learning_rate=0.001),
    loss={'probability': 'mean_squared_error', 'position': 'mean_squared_error'},
    metrics={'probability': 'accuracy', 'position': 'mean_squared_error'}
)

model.summary()

epochChoice = input("PLEASE ENTER NUMBER OF EPOCHS:")
batchChoice = input("PLEASE ENTER NUMBER OF ITEMS IN BATCH:")

batch_size = int(batchChoice)
epochs = int(epochChoice)

train_generator = data_generator(
    image_dir=train_image_dir,
    annotations_file=train_annotations_file,
    batch_size=batch_size,
    image_size=(IMG_HEIGHT, IMG_WIDTH)
)

validation_generator = data_generator(
    image_dir=val_image_dir,
    annotations_file=val_annotations_file,
    batch_size=batch_size,
    image_size=(IMG_HEIGHT, IMG_WIDTH)
)

# Get the number of training and validation samples from the CSV files
num_train_samples = len(pd.read_csv(train_annotations_file))
num_val_samples = len(pd.read_csv(val_annotations_file))

historyProbability = []
historyPosition = []

for epoch in range(epochs):
    print(f"Epoch {epoch + 1} of {epochs}")

    for step, (X_batch, y_batch) in enumerate(train_generator):
        mse = tf.keras.losses.MeanSquaredError()
        #bce = tf.keras.losses.BinaryCrossentropy()

        with tf.GradientTape() as tape:
            prediction_probability, prediction_position = model(X_batch, training=True)
            
            true_probability = y_batch['probability']
            true_position = y_batch['position']
            
            loss_probability = mse(true_probability, prediction_probability)
            
            mask = tf.cast(tf.greater_equal(true_probability, 0.5), dtype=tf.float32)
            mask = tf.expand_dims(mask, axis=-1)    
            
            raw_loss_position = mse(true_position, prediction_position)
            loss_position = raw_loss_position * mask
            
            loss_position = tf.reduce_sum(loss_position)
            if step % 10 == 0:
                print(f"Step {step}: loss_probability = {loss_probability.numpy()}, loss_position = {loss_position.numpy()}")
            total_loss = loss_probability + loss_position
        
        gradients = tape.gradient(total_loss, model.trainable_weights)
        
        model.optimizer.apply_gradients(zip(gradients, model.trainable_weights))
        
        if step % 10 == 0:
            print(f"Step {step}: loss = {total_loss.numpy()}")       

        if step >= (num_train_samples // batch_size):
            break
        
    val_loss = model.evaluate(
        validation_generator,
        steps=num_val_samples // batch_size,
        return_dict=True
    )
    
    # Access individual metrics
    print(f"Validation Classification Accuracy: {val_loss['probability_accuracy'] * 100:.2f}%")
    print(f"Validation Position MSE: {val_loss['position_mean_squared_error']:.4f}")
    
    historyProbability.append(val_loss['probability_accuracy'])
    historyPosition.append(val_loss['position_mean_squared_error'])

#print (history.history)

if trainingSetChoice == 1:
    model.save('full.keras')
elif trainingSetChoice == 2:
    model.save('clean.keras')
elif trainingSetChoice == 3:
    model.save('strict.keras')
elif trainingSetChoice == 4:
    model.save('strictbalanced.keras')
elif trainingSetChoice == 5:
    model.save('small.keras')

    
print ("PROBABILITY HISTORY")
print (historyProbability)
print ("POSITION HISTORY")
print (historyPosition)