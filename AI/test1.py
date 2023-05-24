# -*- coding: utf-8 -*-
import pyodbc 
import sys 
import cv2
import numpy as np
from tensorflow.keras.models import load_model
model =load_model("C:/Users/PC/source/repos/web-net/AI/facenet_keras.h5")
face_cascade = cv2.CascadeClassifier('C:/Users/PC/source/repos/web-net/AI/haarcascade_frontalface_alt2.xml')

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
       gray = cv2.cvtColor(img1, cv2.COLOR_BGR2GRAY)
       faces = face_cascade.detectMultiScale(gray, scaleFactor=1.1, minNeighbors=5, minSize=(30, 30))
       for (x, y, w, h) in faces:
       # Cắt ảnh khuôn mặt từ ảnh gốc
            face_image = img1[y:y+h, x:x+w]
            resized_face = cv2.resize(face_image, (160, 160), interpolation=cv2.INTER_AREA)
       x_train = np.array([resized_face])
       embedding = model.predict(x_train)
       return embedding
class FaceID():
    def __init__(self, email, model):
        self.email = email
        self.model = model
    
    def connect(self, image_filename):
        # Lấy dữ liệu ảnh khuôn mặt từ cơ sở dữ liệu
        retrieved_bytes = cursor.execute("SELECT FaceImg FROM Face WHERE Email = '{}'".format(self.email)).fetchval()
        with open("C:/Users/PC/source/repos/web-net/AI/img/{}.jpg".format(self.email), 'wb') as new_jpg:
            new_jpg.write(retrieved_bytes)

        # Lấy dữ liệu ảnh khuôn mặt để so sánh
        retrieved_bytes1 = cursor.execute("SELECT CheckFaceImg FROM Face WHERE Email = '{}'".format(self.email)).fetchval()
        with open('C:/Users/PC/source/repos/web-net/AI/img/{}_check.jpg'.format(image_filename), 'wb') as new_jpg:
            new_jpg.write(retrieved_bytes1)

        # Chuyển đổi ảnh đầu vào và ảnh để so sánh thành vector biểu diễn khuôn mặt
        image_of_bill = img_to_encoding("C:/Users/PC/source/repos/web-net/AI/img/{}.jpg".format(self.email), self.model)
        unknown_image = img_to_encoding('C:/Users/PC/source/repos/web-net/AI/img/{}_check.jpg'.format(image_filename), self.model)
        # Tính toán khoảng cách Euclid giữa hai vector khuôn mặt
        results = np.linalg.norm(unknown_image - image_of_bill)
        
        # Kiểm tra xem kết quả có dưới 5 hay không
        if results < 5:
            return "yes"
        else:
            return "no"


a = FaceID("{}".format(user), model)
print(a.connect("your_image_filename"))
