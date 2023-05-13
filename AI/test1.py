# -*- coding: utf-8 -*-
import pyodbc 
import sys 
import cv2
import numpy as np
from tensorflow.keras.models import load_model
model =load_model("C:/AI/facenet_keras.h5")

conn = pyodbc.connect(
    'DRIVER={SQL SERVER};'
    'SERVER=DESKTOP-BVAKSHC\\SQLEXPRESS;'
    'DATABASE=Book_Management;'
    'Trust_Connection=yes;'
)
cursor = conn.cursor()
user = str(sys.argv[1])

def img_to_encoding(path, model):
        img1 = cv2.imread(path, 1)
        img = img1[...,::-1]
        dim = (160, 160)
        # resize image
        if(img.shape != (160, 160, 3)):
            img = cv2.resize(img, dim, interpolation = cv2.INTER_AREA)
        x_train = np.array([img])
        embedding = model.predict(x_train)
        return embedding
class FaceID():
    def __init__(self, email,model):
        self.email = email
        self.model = model
    
    def connect(self):
        retrieved_bytes = cursor.execute("SELECT FaceImg FROM Face WHERE Email = '{}'".format(self.email)).fetchval()

        with open('C:/AI/4.jpg', 'wb') as new_jpg:
            new_jpg.write(retrieved_bytes)
        print(f'{len(retrieved_bytes)} bytes retrieved and written to new file')


        retrieved_bytes1 = cursor.execute("SELECT CheckFaceImg FROM Face WHERE Email = '{}'".format(self.email)).fetchval()
        with open('C:/AI/img/4.jpg', 'wb') as new_jpg:    
            new_jpg.write(retrieved_bytes1)
        print(f'{len(retrieved_bytes)} bytes retrieved and written to new file')
        image_of_bill = img_to_encoding('C:/AI/4.jpg',self.model)
        unknown_image = img_to_encoding('C:/AI/img/4.jpg',self.model)
        results = np.linalg.norm(
                unknown_image-image_of_bill)
        if results<5:
            return "yes"
        else: 
            return "no"

a = FaceID("{}".format(user),model)
print(a.connect())