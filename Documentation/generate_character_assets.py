"""Extend the project's original code-native pixel sprite generator with five silhouettes.
No downloaded art. 5 characters x 4 directions x 4 walk poses, plus portraits/evolution icons.
"""
from pathlib import Path
from PIL import Image,ImageDraw,ImageOps
import runpy
root=Path(__file__).resolve().parents[1];out=root/'Assets/PixelSurvivor/Art'
# Load only the drawing function and constants, without rewriting previous generated assets.
source=(root/'Documentation/generate_expansion_assets.py').read_text()
ns={'__file__':str(root/'Documentation/generate_expansion_assets.py')}
exec(source[:source.index("for direction in ('Down','Up','Left','Right'):")],ns)
palettes=[('#39aa9a','#78e9bf'),('#407bd2','#96ddff'),('#bc793c','#f5ce79'),('#7766bc','#e4b4ff'),('#6c436b','#f083b1')]
names=['ARI / BALANCED','VELA / SWIFT','BRAM / POWER','MIRA / MAGE','NOX / REAPER']
def character(k,direction,frame):
 ns['teal'],ns['mint']=palettes[k]
 im=ns['player'](direction,frame).resize((16,16),Image.Resampling.NEAREST)
 d=ImageDraw.Draw(im);step=1 if frame==1 else -1 if frame==3 else 0;bob=-1 if step else 0
 main,light=palettes[k];ink='#122533'
 if k==0: # explorer's diagonal belt
  d.line((6,8+bob,9,11+bob),fill='#b68b5e');d.point((8,10+bob),fill='#fff1b3')
 elif k==1: # feather / trailing scarf
  d.line((8,2+bob,11,0),fill=light,width=1);d.line((11,8+bob,14,8+bob+step),fill=main,width=2)
 elif k==2: # broad armored shoulders and square helmet
  d.rectangle((2,8+bob,5,10+bob),fill=ink);d.rectangle((10,8+bob,13,10+bob),fill=ink)
  d.line((2,8+bob,5,8+bob),fill=light);d.line((10,8+bob,13,8+bob),fill=light)
  d.rectangle((4,1+bob,11,3+bob),fill=main);d.line((5,1+bob,10,1+bob),fill=light)
 elif k==3: # pointed hat and spell staff
  d.polygon([(3,4+bob),(7,0),(10,3+bob),(12,4+bob)],fill=ink)
  d.polygon([(5,3+bob),(7,0),(9,3+bob)],fill=main);d.line((3,4+bob,12,4+bob),fill=light)
  d.line((13,7+bob,13,14),fill='#c99858');d.rectangle((12,6+bob,14,8+bob),fill=light)
 elif k==4: # dark hooded cloak and crescent blade
  d.rectangle((4,8+bob,5,12),fill=main);d.rectangle((10,8+bob,11,12),fill=main)
  d.line((2,7,2,14),fill='#b7bccc');d.arc((0,3+bob,6,9+bob),180,335,fill=light,width=2)
  d.line((5,3+bob,10,3+bob),fill=ink)
 return im.resize((32,32),Image.Resampling.NEAREST)
for k in range(5):
 for direction in ('Down','Up','Left','Right'):
  for frame in range(4):character(k,direction,frame).save(out/f'Character{k}_{direction}_{frame}.png')
 character(k,'Down',0).save(out/f'Portrait{k}.png')
for k in range(8):
 im=Image.new('RGBA',(32,32));d=ImageDraw.Draw(im);d.polygon([(16,0),(31,16),(16,31),(0,16)],outline='#ffe3a0',width=2)
 source=Image.open(out/f'Weapon{k}.png');im.alpha_composite(source)
 d=ImageDraw.Draw(im);d.line((3,2,3,7),fill='#fff8d9');d.line((1,4,5,4),fill='#fff8d9');d.rectangle((26,25,29,28),fill='#ffe3a0')
 im.save(out/f'EvolvedWeapon{k}.png')
sheet=Image.new('RGB',(1000,410),'#0e1b25');d=ImageDraw.Draw(sheet)
d.text((18,12),'SOURCE ART PREVIEW - five distinct characters / four walking directions',fill='#9ae7d5')
for k in range(5):
 d.text((k*200+10,50),names[k],fill='white')
 for i,direction in enumerate(('Down','Up','Left','Right')):
  im=character(k,direction,1).resize((72,72),Image.Resampling.NEAREST);sheet.paste(im,(k*200+60,85+i*78),im)
sheet.save(root/'Documentation/CharacterPreview.png')
frames=[]
for direction in ('Down','Right','Up','Left'):
 for repeat in range(2):
  for f in range(4):
   im=Image.new('RGB',(800,220),'#0e1b25');d=ImageDraw.Draw(im)
   for k in range(5):
    sprite=character(k,direction,f).resize((128,128),Image.Resampling.NEAREST);im.paste(sprite,(k*160+16,30),sprite);d.text((k*160+10,180),names[k],fill='white')
   frames.append(im)
frames[0].save(root/'Documentation/CharactersWalk.gif',save_all=True,append_images=frames[1:],duration=120,loop=0)
print('80 walking frames, 5 portraits, 8 evolved icons generated')
