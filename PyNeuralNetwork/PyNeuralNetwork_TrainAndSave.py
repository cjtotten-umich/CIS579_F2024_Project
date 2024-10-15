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
import os
os.environ['TF_CPP_MIN_LOG_LEVEL'] = '2' 

mixed_precision.set_global_policy('mixed_float16')

IMG_HEIGHT, IMG_WIDTH = 512, 512

def custom_data_generator(image_dir, annotations_file, batch_size, image_size):
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
                
                probability_labels.append(row['sign'])
                position_labels.append([row['x'], row['y']])
                
            
            X = np.array(images)
            y_probability = np.array(probability_labels)
            y_probability = np.reshape(y_probability, (-1, 1))
            
            y_position = np.array(position_labels)
             
            yield X, {'probability': y_probability, 'position': y_position}

def Custom_BoundingBox_Loss(y_true, y_pred):
    true_probability = y_true[:, 0] 
        
    true_location = y_true[:, 1:] 
        
    # Print the predicted and true bounding boxes for debugging
    # tf.print("class_true:", class_true)
    # tf.print("true_bbox:", true_bbox)
    # tf.print("y_true:", y_pred)
    # tf.print("y_pred:", y_pred)
    
    probability_mask = K.cast(K.greater(true_probability, 0.5), K.floatx())
        
    location_loss = K.mean(K.square(y_pred - true_location), axis=-1)
        
    masked_loss = location_loss * probability_mask
        
    masked_loss = tf.where(tf.math.is_finite(masked_loss), masked_loss, tf.zeros_like(masked_loss))
        
    return masked_loss
    
train_image_dir = '/tmp/TrainingData/images'
train_annotations_file = '/tmp/TrainingData/annotations.csv'
val_image_dir = '/tmp/TrainingData/images'
val_annotations_file = '/tmp/TrainingData/annotations.csv'

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

position_output = Dense(2, activation='linear', name='position')(x)

model = Model(inputs=input_layer, outputs=[probability_output, position_output])

model.compile(
    optimizer=Adam(learning_rate=0.001),
    loss={'probability': 'binary_crossentropy', 'position': 'mean_squared_error'},
    metrics={'probability': 'accuracy', 'position': 'mean_squared_error'}
)

#model.summary()

# Set batch size and number of epochs
batch_size = 16
epochs = 50

# Prepare data generators for training and validation
train_generator = custom_data_generator(
    image_dir=train_image_dir,
    annotations_file=train_annotations_file,
    batch_size=batch_size,
    image_size=(IMG_HEIGHT, IMG_WIDTH)
)

validation_generator = custom_data_generator(
    image_dir=val_image_dir,
    annotations_file=val_annotations_file,
    batch_size=batch_size,
    image_size=(IMG_HEIGHT, IMG_WIDTH)
)

# Get the number of training and validation samples from the CSV files
num_train_samples = len(pd.read_csv(train_annotations_file))
num_val_samples = len(pd.read_csv(val_annotations_file))

for epoch in range(epochs):
    print(f"Epoch {epoch + 1}/{epochs}")
    
    for step, (X_batch, y_batch) in enumerate(train_generator):
        mse = tf.keras.losses.MeanSquaredError()
        bce = tf.keras.losses.BinaryCrossentropy()

        with tf.GradientTape() as tape:
            y_pred_prob, y_pred_pos = model(X_batch, training=True)
            
            y_true_prob = y_batch['probability']
            y_true_pos = y_batch['position']
            
            loss_prob = bce(y_true_prob, y_pred_prob)
            
            mask = tf.cast(tf.equal(y_true_prob, 1), dtype=tf.float32) 
            mask = tf.expand_dims(mask, axis=-1)    
            
            raw_loss_pos = mse(y_true_pos, y_pred_pos)
            loss_pos = raw_loss_pos * mask
            
            loss_pos = tf.reduce_sum(loss_pos)
    
            total_loss = loss_prob + loss_pos
        
        gradients = tape.gradient(total_loss, model.trainable_weights)
        
        model.optimizer.apply_gradients(zip(gradients, model.trainable_weights))
        
        # Print progress
        if step % 10 == 0:
            print(f"Step {step}: loss = {total_loss.numpy()}")
        
        # If the number of steps per epoch is reached, break
        if step >= (num_train_samples // batch_size):
            break

#print (history.history)

model.save('model.keras')

# Evaluate the model on validation data
#val_loss, val_class_acc = model.evaluate(validation_generator, steps=num_val_samples // batch_size)
#print(f"Validation Classification Accuracy: {val_class_acc * 100:.2f}%")