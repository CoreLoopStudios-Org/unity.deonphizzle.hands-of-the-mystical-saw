import os
import json
import urllib.request
import urllib.parse
from concurrent.futures import ThreadPoolExecutor

def translate_text(text):
    if not text.strip(): return text
    try:
        url = 'https://translate.googleapis.com/translate_a/single?client=gtx&sl=bn&tl=en&dt=t&q=' + urllib.parse.quote(text)
        req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
        response = urllib.request.urlopen(req)
        data = json.loads(response.read().decode('utf-8'))
        translated = "".join(part[0] for part in data[0] if part[0])
        return translated
    except Exception as e:
        print(f"Failed to translate {text}: {e}")
        return text

def main():
    print("Loading strings...")
    with open('bengali_strings.json', 'r', encoding='utf-8') as f:
        data = json.load(f)
    
    unique_texts = set()
    for file_path, items in data.items():
        for item in items:
            unique_texts.add(item['text'])
    
    unique_texts = list(unique_texts)
    print(f"Found {len(unique_texts)} unique strings to translate.")
    
    translations = {}
    print("Translating...")
    with ThreadPoolExecutor(max_workers=10) as executor:
        results = executor.map(translate_text, unique_texts)
        for original, translated in zip(unique_texts, results):
            translations[original] = translated
    
    print("Translations complete. Replacing in files...")
    
    for file_path, items in data.items():
        if not os.path.exists(file_path):
            continue
            
        with open(file_path, 'r', encoding='utf-8') as f:
            lines = f.readlines()
            
        changed = False
        for item in items:
            line_idx = item['line']
            original_text = item['text']
            translated_text = translations.get(original_text, original_text)
            if translated_text != original_text:
                # Replace the exact original text with translated in the line
                # Note: replacing original text instead of whole line to keep indentation
                lines[line_idx] = lines[line_idx].replace(original_text, translated_text)
                changed = True
                
        if changed:
            with open(file_path, 'w', encoding='utf-8') as f:
                f.writelines(lines)
            print(f"Updated {file_path}")
            
    print("All done!")

if __name__ == "__main__":
    main()
