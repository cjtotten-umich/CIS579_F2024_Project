import os
import numpy as np
import pandas as pd
from tensorflow.keras.models import Model
from tensorflow.keras.layers import Conv2D, MaxPooling2D, Flatten, Dense, Dropout, BatchNormalization, Input
from tensorflow.keras.preprocessing.image import load_img, img_to_array
from tensorflow.keras.optimizers import Adam

# Set image size
IMG_HEIGHT, IMG_WIDTH = 512, 512
BBOX_OUTPUT = 4  # (x_min, y_min, x_max, y_max)

# Define the custom data generator
def custom_data_generator(image_dir, annotations_file, batch_size, image_size):
    # Read the annotations CSV file
    annotations = pd.read_csv(annotations_file)
    
    num_samples = len(annotations)
    
    while True:
        # Shuffle the annotations at the start of each epoch
        annotations = annotations.sample(frac=1).reset_index(drop=True)

        for offset in range(0, num_samples, batch_size):
            batch_samples = annotations.iloc[offset:offset+batch_size]
            images = []
            class_labels = []
            bbox_labels = []
            
            for _, row in batch_samples.iterrows():
                # Load and preprocess the image
                img_path = os.path.join(image_dir, row['filename'])
                img = load_img(img_path, target_size=image_size)
                img = img_to_array(img) / 255.0  # Normalize to [0, 1]
                images.append(img)
                
                # Get the class label and bounding box
                class_label = row['class']
                bbox = [row['x_min'], row['y_min'], row['x_max'], row['y_max']]
                class_labels.append(class_label)
                bbox_labels.append(bbox)
            
            # Convert lists to numpy arrays
            X = np.array(images)
            y_class = np.array(class_labels)
            y_bbox = np.array(bbox_labels)
            
            # Yield the batch of images, class labels, and bounding boxes
            yield X, (y_class, y_bbox)

# Set up file paths for training and validation data
train_image_dir = '..\\TrainingData\\images'
train_annotations_file = '..\\TrainingData\\annotations.csv'
val_image_dir = '..\\TrainingData\\images'
val_annotations_file = '..\\TrainingData\\annotations.csv'

# Build the CNN model for detecting signs and predicting bounding boxes
input_layer = Input(shape=(IMG_HEIGHT, IMG_WIDTH, 3))

# First convolutional layer
x = Conv2D(32, (3, 3), activation='relu')(input_layer)
x = BatchNormalization()(x)
x = MaxPooling2D(pool_size=(2, 2))(x)

# Second convolutional layer
x = Conv2D(64, (3, 3), activation='relu')(x)
x = BatchNormalization()(x)
x = MaxPooling2D(pool_size=(2, 2))(x)

# Third convolutional layer
x = Conv2D(128, (3, 3), activation='relu')(x)
x = BatchNormalization()(x)
x = MaxPooling2D(pool_size=(2, 2))(x)

# Fourth convolutional layer
x = Conv2D(256, (3, 3), activation='relu')(x)
x = BatchNormalization()(x)
x = MaxPooling2D(pool_size=(2, 2))(x)

# Flatten the output and feed into fully connected layers
x = Flatten()(x)
x = Dense(512, activation='relu')(x)
x = Dropout(0.5)(x)

# Output for bounding box (4 values)
bbox_output = Dense(BBOX_OUTPUT, activation='sigmoid', name='bbox')(x)

# Output for classification (1 value for binary classification)
class_output = Dense(1, activation='sigmoid', name='class')(x)

# Define the model
model = Model(inputs=input_layer, outputs=[class_output, bbox_output])

# Compile the model with a combined loss
model.compile(optimizer=Adam(learning_rate=0.001),
              loss={
                  'class': 'binary_crossentropy',  # Classification loss
                  'bbox': 'mean_squared_error'     # Bounding box regression loss
              },
              metrics={
                  'class': 'accuracy',             # Classification accuracy
                  'bbox': 'mean_squared_error'     # Bounding box mean squared error
              })

# Print the model summary
model.summary()

# Set batch size and number of epochs
batch_size = 3
epochs = 25

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

# Train the model
history = model.fit(
    train_generator,
    steps_per_epoch=num_train_samples // batch_size,
    validation_data=validation_generator,
    validation_steps=num_val_samples // batch_size,
    epochs=epochs
)

# Save the trained model
model.save('model.keras')

# Evaluate the model on validation data
# val_loss, val_class_acc, val_bbox_mse = model.evaluate(validation_generator, steps=num_val_samples // batch_size)
# print(f"Validation Classification Accuracy: {val_class_acc * 100:.2f}%")
# print(f"Validation Bounding Box MSE: {val_bbox_mse:.4f}")