"""Small original code-drawn sprites. Requires Pillow only; optional source tool."""
from pathlib import Path
from PIL import Image, ImageDraw
import math,colorsys
root=Path(__file__).resolve().parents[1];out=root/'Assets/PixelSurvivor/Art'
def create(name,draw):
 im=Image.new('RGBA',(32,32));d=ImageDraw.Draw(im);draw(d);im.save(out/(name+'.png'))
ink='#122533'
create('AttackRing',lambda d:d.ellipse((1,1,30,30),outline='white',width=2))
def chest(d):
 d.rectangle((3,8,28,27),fill=ink);d.rectangle((5,10,26,25),fill='#bb8347');d.rectangle((5,10,26,15),fill='#e9ba65');d.rectangle((3,15,28,18),fill=ink);d.rectangle((14,14,18,21),fill='#fff1ae');d.rectangle((7,11,9,25),fill='#f9d581');d.rectangle((23,11,25,25),fill='#f9d581')
create('TreasureChest',chest)
def crate(d):
 d.rectangle((3,4,28,28),fill=ink);d.rectangle((5,6,26,26),fill='#91664c');d.rectangle((7,8,24,24),fill='#614c44');d.line((6,7,25,25),fill='#bc986a',width=3);d.line((25,7,6,25),fill='#bc986a',width=3);d.rectangle((14,13,18,18),fill='#78e9bf')
create('SupplyCrate',crate)
create('FieldItem',lambda d:(d.polygon([(16,3),(28,12),(25,26),(7,26),(4,12)],fill=ink),d.polygon([(16,6),(25,13),(22,23),(10,23),(7,13)],fill='white'),d.rectangle((14,11,18,20),fill='#a8b2c1')))
def drone(d):
 d.rectangle((3,8,9,19),fill=ink);d.rectangle((23,8,29,19),fill=ink);d.rectangle((8,7,24,23),fill=ink);d.rectangle((10,9,22,20),fill='white');d.rectangle((12,12,20,16),fill='#39aa9a');d.rectangle((4,10,7,17),fill='#78e9bf');d.rectangle((25,10,28,17),fill='#78e9bf')
create('SupportDrone',drone)
for i in range(8):
 color=tuple(int(v*255) for v in colorsys.hsv_to_rgb(i/8,.55,1))
 def weapon(d,k=i,col=color):
  d.rectangle((1,1,30,30),fill=ink);d.rectangle((3,3,28,28),outline=col,width=1)
  if k==0:d.polygon([(18,5),(9,18),(15,18),(12,27),(24,12),(18,12)],fill=col)
  elif k==1:d.polygon([(23,5),(10,19),(15,22)],fill=col);d.line((7,18,17,27),fill='white',width=2)
  elif k==2:d.line([(6,24),(10,9),(23,8),(24,16),(16,21)],fill=col,width=3)
  elif k==3:d.ellipse((7,7,24,24),outline=col,width=2);d.rectangle((13,3,18,8),fill='white')
  elif k==4:d.arc((6,5,26,24),180,340,fill=col,width=4);d.line((21,12,9,26),fill='white',width=2)
  elif k==5:d.line([(23,5),(10,15),(20,15),(8,27)],fill=col,width=3)
  elif k==6:
   for a in range(8):
    t=a*math.pi/4;d.line((16+math.cos(t)*5,16+math.sin(t)*5,16+math.cos(t)*11,16+math.sin(t)*11),fill=col,width=2)
   d.ellipse((12,12,20,20),fill='white')
  else:d.ellipse((7,6,23,22),outline=col,width=3);d.polygon([(24,16),(19,22),(27,24)],fill='white')
 create('Weapon'+str(i),weapon)
 def passive(d,k=i,col=color):
  d.polygon([(16,1),(30,16),(16,30),(1,16)],fill=ink);d.polygon([(16,4),(27,16),(16,27),(4,16)],outline=col,width=2)
  # Compact numeric-independent symbols for eight distinct passive items.
  if k==0:d.polygon([(16,7),(10,18),(15,17),(12,24),(23,13),(17,13)],fill=col)
  elif k==1:d.rectangle((13,8,19,24),fill=col);d.rectangle((8,13,24,19),fill=col)
  elif k==2:d.ellipse((9,9,23,23),outline=col,width=2);d.line((16,10,16,16,21,16),fill='white',width=2)
  elif k==3:d.ellipse((8,8,22,22),outline=col,width=2);d.line((20,20,25,25),fill=col,width=3)
  elif k==4:d.rectangle((12,12,20,24),fill=col);d.polygon([(16,5),(12,10),(20,10)],fill='#fff4bd')
  elif k==5:d.polygon([(11,8),(19,8),(19,19),(24,19),(24,23),(9,23)],fill=col)
  elif k==6:d.arc((9,8,23,24),0,180,fill=col,width=4);d.rectangle((9,10,12,17),fill='white');d.rectangle((20,10,23,17),fill='white')
  else:d.polygon([(11,9),(16,16),(11,23),(6,16)],fill=col);d.polygon([(21,9),(26,16),(21,23),(16,16)],fill='white')
 create('Passive'+str(i),passive)
sheet=Image.new('RGB',(800,240),'#0e1b25');d=ImageDraw.Draw(sheet)
for row,prefix in enumerate(('Weapon','Passive')):
 for i in range(8):
  im=Image.open(out/(prefix+str(i)+'.png')).resize((64,64),Image.Resampling.NEAREST);sheet.paste(im,(20+i*98,25+row*110),im);d.text((20+i*98,92+row*110),prefix+str(i),fill='white')
sheet.save(root/'Documentation/CampaignIcons.png')
print('21 original PNG assets generated')
