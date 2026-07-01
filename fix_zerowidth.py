import os

zero_width = ['\u200b', '\u200c', '\u200d', '\ufeff']
count = 0

for root, dirs, files in os.walk('Assets/Scripts'):
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(root, file)
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
            
            new_content = content
            for z in zero_width:
                new_content = new_content.replace(z, '')
                
            if new_content != content:
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(new_content)
                print(f"Fixed zero-width chars in {filepath}")
                count += 1

print(f"Total files fixed: {count}")
