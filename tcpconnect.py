import socket
import json
import time
from termios import tcflush, TCIFLUSH
import sys

TCP_IP = '192.168.7.2'
TCP_PORT = 5005
BUFFER_SIZE = 512  # Normally 1024, but we want fast response

while 1:
    s = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    s.bind((TCP_IP, TCP_PORT))
    s.listen(1)

    conn, addr = s.accept()
    print 'Connection address:', addr
    while 1:
        data = conn.recv(BUFFER_SIZE)
        if not data: break
        #print "received data:", data
        controllerdata = json.loads(str(data))
        for k,v in controllerdata.items(): 
            print str(k) + " " + str(v)
        #print "controllerdata:", controllerdata
        conn.send(data)  # echo
    conn.close()
    time.sleep(.001)
    tcflush(sys.stdin, TCIFLUSH)
    #digitalWrite(, elavtorArm)

