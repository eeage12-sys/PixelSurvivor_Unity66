"""Original code-native pixel assets; requires Python + Pillow. Run from project root."""
from pathlib import Path
from PIL import Image, ImageDraw
import random
root = Path(__file__).resolve().parents[1]
out = root/'Assets/PixelSurvivor/Art'
out.mkdir(parents=True, exist_ok=True)
palette={'.':(0,0,0,0),'o':'#122533','d':'#285065','t':'#39aa9a','m':'#78e9bf','w':'#ecfce7','s':'#e9b988','h':'#76534b','b':'#4981a4','p':'#a989d5','l':'#d5b2f1','r':'#714d96','c':'#7bddff','u':'#377cdb','y':'#f9edac'}
def matrix(name,rows,scale=1):
 im=Image.new('RGBA',(len(rows[0]),len(rows)))
 for y,row in enumerate(rows):
  assert len(row)==im.width,(name,y,len(row))
  for x,c in enumerate(row):
   if c!='.': im.putpixel((x,y),tuple(int(palette[c][i:i+2],16) for i in (1,3,5))+(255,))
 if scale!=1: im=im.resize((im.width*scale, im.height*scale),Image.Resampling.NEAREST)
 im.save(out/(name+'.png'))
matrix('Player',[
'................','.....oooooo.....','....otttttto....','...otmmmmttto...',
'...otmwwwmtto...','...otssssstto...','....oswoswo.....','....ossssso.....',
'...ootttttoo....','..odtmttttmdo...','..ostmttttmso...','...oottttoo.....',
'....oddddo......','....odoodo......','...oodoodoo.....','...ooo..ooo.....'],2)
matrix('Enemy',[
'................','................','.....oooo.......','....opplro......',
'...opllllpro....','..opllllllpro...','..opllwllwlro...','.opllwowwowlro..',
'.opllrowworlro..','.oplllrrrrlpro..','.oppllllllppro..','..oppppppppro...',
'..orpprrpprro...','...oooo.oooo....','................','................'],2)
matrix('Projectile',['..oooo..','.ouccuo.','oucyycuo','ocyyywco','ocyywwco','oucywcuo','.ouccuo.','..oooo..'])
matrix('ExperienceGem',['............','.....oo.....','....ouuo....','...oucwuo...','..ouccwcuo..','.oucccwccuo.','.ouccccccuo.','..ouccccuo..','...ouccuo...','....ouuo....','.....oo.....','............'])
matrix('Tuft',['................','................','................','................','................','................','.......d........','...d...d..d.....','....d..t..d.....','....dt.t.dt.....','.....t.ttd......','......ttt.......','................','................','................','................'])
Image.new('RGBA',(1,1),'white').save(out/'White.png')
im=Image.new('RGBA',(64,64),'#182f38');d=ImageDraw.Draw(im);rng=random.Random(66)
for i in range(75):
 x,y=rng.randrange(64),rng.randrange(64)
 d.rectangle((x,y,x+1,y),fill=rng.choice(['#1b343b','#203a40','#162c35']))
# Discreet grid corners give direction without noisy tiles.
d.point([(0,0),(1,0),(0,1)],fill='#29434a')
im.save(out/'Ground.png')
# Contact sheet is a source-asset preview, not a Unity play screenshot.
sheet=Image.new('RGB',(680,240),'#0e1b25');d=ImageDraw.Draw(sheet)
for i,name in enumerate(['Player','Enemy','Projectile','ExperienceGem']):
 im=Image.open(out/(name+'.png')); im=im.resize((im.width*4,im.height*4),Image.Resampling.NEAREST)
 x=i*170+(170-im.width)//2;y=30+(130-im.height)//2
 sheet.paste(im,(x,y),im);d.text((i*170+18,190),name,fill='#9ae7d5')
sheet.save(root/'Documentation/SpritePreview.png')
