import ftplib
import os
import time

def partial_deploy():
    HOST = 'windows11.aspone.cz'
    USER = 'EkoBio.org_lordkikin'
    PASS = 'Brzsilpot7!'
    LOCAL_ROOT = 'publish'
    FILES_TO_UPLOAD = ['ScioApp.dll', 'ScioApp.exe', 'web.config']

    ftp = ftplib.FTP(HOST)
    ftp.login(USER, PASS)
    ftp.cwd('/www/scio')

    for file in FILES_TO_UPLOAD:
        local_path = os.path.join(LOCAL_ROOT, file)
        if os.path.exists(local_path):
            print(f"Uploading {file}...")
            # Rename if dll/exe to avoid lock
            if file.endswith(('.dll', '.exe')):
                try:
                    bak_name = f"{file}.{int(time.time())}.bak"
                    ftp.rename(file, bak_name)
                    print(f"  Renamed {file} to {bak_name}")
                except Exception as e:
                    print(f"  Could not rename {file}: {e}")
            
            with open(local_path, 'rb') as f:
                ftp.storbinary(f"STOR {file}", f)
    
    ftp.quit()
    print("Partial deploy finished.")

if __name__ == "__main__":
    partial_deploy()
