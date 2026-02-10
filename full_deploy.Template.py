import ftplib
import os
import time

# TEMPLATE FOR DEPLOYMENT SCRIPT
# Copy correctly filled version from deploy_scio.py (local only, do not git)

def deploy():
    HOST = 'YOUR_FTP_HOST'
    USER = 'YOUR_FTP_USER'
    PASS = 'YOUR_FTP_PASSWORD'
    LOCAL_ROOT = 'publish'
    REMOTE_ROOT = '/www/scio'

    ftp = ftplib.FTP(HOST)
    ftp.login(USER, PASS)
    
    def ensure_dir(path):
        parts = path.split('/')
        current = REMOTE_ROOT
        ftp.cwd(current)
        for part in parts:
            if not part: continue
            try:
                ftp.mkd(part)
            except:
                pass
            ftp.cwd(part)
            current += '/' + part

    for root, dirs, files in os.walk(LOCAL_ROOT):
        rel_dir = os.path.relpath(root, LOCAL_ROOT).replace('\\', '/')
        if rel_dir == '.': rel_dir = ''
        
        for file in files:
            local_path = os.path.join(root, file)
            remote_path = (rel_dir + '/' + file) if rel_dir else file
            
            print(f"Uploading {remote_path}...")
            
            # Special handling for locked files
            is_locked_type = file.lower().endswith(('.dll', '.exe'))
            if is_locked_type:
                try:
                    ensure_dir(rel_dir)
                    bak_name = f"{file}.{int(time.time())}.bak"
                    ftp.rename(file, bak_name)
                    print(f"  Renamed {file} to {bak_name}")
                except:
                    pass
            
            ensure_dir(rel_dir)
            with open(local_path, 'rb') as f:
                ftp.storbinary(f"STOR {file}", f)
            
    ftp.quit()
    print("Full deploy finished.")

if __name__ == "__main__":
    deploy()
