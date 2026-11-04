"""Original code-native pixel art extension. Python + Pillow; no external artwork.
All animation frames are individual PNGs, so Unity needs no slicing package.
"""
from pathlib import Path
from PIL import Image, ImageDraw, ImageOps
root = Path(__file__).resolve().parents[1]
out = root / 'Assets/PixelSurvivor/Art'
out.mkdir(parents=True, exist_ok=True)
ink='#122533'; teal='#39aa9a'; mint='#78e9bf'; skin='#e9b988'; dark='#285065'; light='#ecfce7'

def player(direction, frame):
    im=Image.new('RGBA',(16,16)); d=ImageDraw.Draw(im)
    step=1 if frame==1 else -1 if frame==3 else 0
    bob=-1 if step else 0
    # Boots stride independently, at integer pixels.
    for x,offset in ((5,step),(9,-step)):
        d.rectangle((x,11,x+2,14+offset),fill=ink)
        d.rectangle((x+1,12,x+1,13+offset),fill=dark)
    if direction in ('Down','Up'):
        # Arms swing opposite each leg.
        for x,offset in ((3,-step),(11,step)):
            d.rectangle((x,8+offset,x+1,11+offset),fill=ink)
            d.point((x,10+offset),fill=skin if direction=='Down' else teal)
        d.rectangle((5,7+bob,10,12+bob),fill=ink)
        d.rectangle((6,8+bob,9,11+bob),fill=teal)
        d.rectangle((4,2+bob,11,7+bob),fill=ink)
        d.rectangle((5,1+bob,10,8+bob),fill=ink)
        d.rectangle((5,2+bob,10,6+bob),fill=teal)
        d.line((6,2+bob,9,2+bob),fill=mint)
        if direction=='Down':
            d.rectangle((5,5+bob,10,7+bob),fill=skin)
            d.point((6,5+bob),fill=light);d.point((9,5+bob),fill=light)
            d.point((6,6+bob),fill=ink);d.point((9,6+bob),fill=ink)
            d.line((6,8+bob,10,8+bob),fill=mint)
        else:
            d.rectangle((6,4+bob,9,7+bob),fill=teal)
            d.line((7,3+bob,8,3+bob),fill=mint)
            d.rectangle((6,9+bob,9,11+bob),fill=dark)
            d.line((7,9+bob,8,9+bob),fill=mint)
    else:
        d.rectangle((5,8+bob,10,12+bob),fill=ink)
        d.rectangle((6,9+bob,9,11+bob),fill=teal)
        d.rectangle((4,2+bob,10,7+bob),fill=ink)
        d.rectangle((5,1+bob,9,8+bob),fill=ink)
        d.rectangle((5,2+bob,9,6+bob),fill=teal)
        d.line((6,2+bob,8,2+bob),fill=mint)
        d.rectangle((9,4+bob,11,7+bob),fill=skin)
        d.point((10,4+bob),fill=ink)
        d.rectangle((7+step,8+bob,8+step,11+bob),fill=ink)
        d.point((8+step,10+bob),fill=skin)
        if direction=='Left': im=ImageOps.mirror(im)
    return im.resize((32,32),Image.Resampling.NEAREST)
for direction in ('Down','Up','Left','Right'):
    for frame in range(4): player(direction,frame).save(out/f'Player_{direction}_{frame}.png')

def moth():
    im=Image.new('RGBA',(16,16));d=ImageDraw.Draw(im)
    d.polygon([(1,2),(5,4),(7,7),(6,12),(3,11),(0,7)],fill=ink)
    d.polygon([(14,2),(10,4),(8,7),(9,12),(12,11),(15,7)],fill=ink)
    d.polygon([(2,4),(5,5),(6,8),(4,10),(2,8)],fill='#ed9568')
    d.polygon([(13,4),(10,5),(9,8),(11,10),(13,8)],fill='#ed9568')
    d.line((2,5,4,7),fill='#ffe6a2',width=1);d.line((13,5,11,7),fill='#ffe6a2',width=1)
    d.rectangle((6,5,9,12),fill=ink);d.rectangle((7,5,8,11),fill='#916b9d')
    d.point((6,6),fill='#7bddff');d.point((9,6),fill='#7bddff')
    d.line((6,4,5,2),fill=ink);d.line((9,4,10,2),fill=ink)
    return im.resize((32,32),Image.Resampling.NEAREST)
def beetle():
    im=Image.new('RGBA',(16,16));d=ImageDraw.Draw(im)
    for y in (6,9,12):
        d.line((1,y-1,4,y+1),fill=ink,width=2);d.line((14,y-1,11,y+1),fill=ink,width=2)
    d.rectangle((5,1,6,4),fill=ink);d.rectangle((9,1,10,4),fill=ink)
    d.ellipse((2,3,13,14),fill=ink);d.ellipse((3,4,12,13),fill='#a56136')
    d.ellipse((4,4,11,11),fill='#dfaa55');d.line((7,5,7,13),fill=ink)
    d.rectangle((5,4,10,6),fill=ink);d.point((6,5),fill='#eaffc5');d.point((9,5),fill='#eaffc5')
    d.line((4,8,4,10),fill='#ffe6a2');d.point((10,8),fill='#ffe6a2')
    return im.resize((32,32),Image.Resampling.NEAREST)
moth().save(out/'EnemyMoth.png');beetle().save(out/'EnemyBeetle.png')
# Inspection sheet: authored asset preview, not a Unity runtime capture.
sheet=Image.new('RGB',(760,610),'#0e1b25');d=ImageDraw.Draw(sheet)
d.text((20,15),'SOURCE SPRITES | 4 directions x 4 frames | 3 enemy types',fill='#9ae7d5')
for row,direction in enumerate(('Down','Up','Left','Right')):
    d.text((20,70+row*105),direction,fill='white')
    for frame in range(4):
        im=player(direction,frame).resize((96,96),Image.Resampling.NEAREST)
        sheet.paste(im,(130+frame*140,38+row*105),im)
for i,name in enumerate(('Enemy','EnemyMoth','EnemyBeetle')):
    im=Image.open(out/(name+'.png')).resize((96,96),Image.Resampling.NEAREST)
    sheet.paste(im,(115+i*220,480),im);d.text((110+i*220,585),name,fill='#9ae7d5')
sheet.save(root/'Documentation/ExpansionSpritePreview.png')
frames=[]
for direction in ('Down','Right','Up','Left'):
    for cycle in range(3):
        for i in range(4):
            canvas=Image.new('RGB',(240,260),'#0e1b25');d=ImageDraw.Draw(canvas)
            im=player(direction,i).resize((192,192),Image.Resampling.NEAREST)
            canvas.paste(im,(24,24),im);d.text((25,230),'Walk preview: '+direction,fill='#9ae7d5');frames.append(canvas)
frames[0].save(root/'Documentation/WalkPreview.gif',save_all=True,append_images=frames[1:],duration=100,loop=0)
print('18 PNGs + source preview PNG/GIF generated')
