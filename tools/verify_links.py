import os, re

repo = r'C:\Users\User\Documents\win-tweaker'
link_re = re.compile(r'\[([^\]]+)\]\(([^)]+)\)')
img_re = re.compile(r'src=["\']([^"\']+)["\']')

broken = []
files_to_check = ['README.md', 'README.ru.md', 'AGENTS.md']

# also check all docs
for root, dirs, files in os.walk(os.path.join(repo, 'docs')):
    for f in files:
        if f.endswith('.md'):
            files_to_check.append(os.path.relpath(os.path.join(root, f), repo))

for relpath in files_to_check:
    fpath = os.path.join(repo, relpath)
    base_dir = os.path.dirname(fpath)
    with open(fpath, 'r', encoding='utf-8', errors='ignore') as fp:
        txt = fp.read()
    
    for text, target in link_re.findall(txt):
        if target.startswith('http') or target.startswith('#') or target.startswith('mailto'):
            continue
        clean = target.split('#')[0]
        if not clean:
            continue
        resolved = os.path.normpath(os.path.join(base_dir, clean))
        if not os.path.exists(resolved):
            broken.append((relpath, text, target, os.path.relpath(resolved, repo)))
            
    for src in img_re.findall(txt):
        if src.startswith('http'):
            continue
        clean = src.split('#')[0]
        if not clean:
            continue
        resolved = os.path.normpath(os.path.join(base_dir, clean))
        if not os.path.exists(resolved):
            broken.append((relpath, '<img src>', src, os.path.relpath(resolved, repo)))

if broken:
    print(f"FAILED: Found {len(broken)} broken link(s):")
    for f, t, tgt, r in broken:
        print(f"  {f}: [{t}]({tgt}) -> resolved path '{r}' not found")
    exit(1)
else:
    print(f"SUCCESS: Checked {len(files_to_check)} markdown files, all links and images are 100% VALID!")
    exit(0)
