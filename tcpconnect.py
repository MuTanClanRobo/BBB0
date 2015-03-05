#!/usr/bin/env python

import socket
import json

TCP_IP = '192.168.7.2'
TCP_PORT = 5005
BUFFER_SIZE = 256  # Normally 1024, but we want fast response



s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
s.bind((TCP_IP, TCP_PORT))
s.listen(1)

conn, addr = s.accept()
print 'Connection address:', addr
data = ""
while 1:
    while len(data) == 0:
        data = data + conn.recv(BUFFER_SIZE)
    print "received data:", data
#    controllerdata = json.loads(str(data)) #Assume that data is the JSON string which we just got from our 
#    print "this is the data", controllerdata
    #if not data: break
    #conn.send(data)  # echo
    



#conn.close()
