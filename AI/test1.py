# -*- coding: utf-8 -*-
import pyodbc 
import sys 
import cv2
import numpy as np
from tensorflow.keras.models import load_model
model =load_model("C:/Users/PC/source/repos/web-net/AI/facenet_keras.h5")

conn = pyodbc.connect(
    'DRIVER={SQL SERVER};'
    'SERVER=DESKTOP-BVAKSHC\\SQLEXPRESS;'
    'DATABASE=Book_Management;'
    'Trust_Connection=yes;'
)
cursor = conn.cursor()
user = str(sys.argv[1])

def img_to_encoding(path, model):
         # Đọc ảnh và chuyển đổi từ BGR sang RGB
        img1 = cv2.imread(path, 1)
        img = img1[...,::-1]
        dim = (160, 160)
        # Resize ảnh thành kích thước 160x160 nếu cần
        if(img.shape != (160, 160, 3)):
            img = cv2.resize(img, dim, interpolation = cv2.INTER_AREA)
        # Chuẩn bị dữ liệu đầu vào cho mô hình facenet
        x_train = np.array([img])
        # Trích xuất vector biểu diễn khuôn mặt
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
