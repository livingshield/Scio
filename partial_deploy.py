import ftplib
import os

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
                    ftp.rename(file, f"{file}.bak")
                except:
                    pass
            with open(local_path, 'rb') as f:
                ftp.storbinary(f"STOR {file}", f)
    
    ftp.quit()
    print("Partial deploy finished.")

if __name__ == "__main__":
    partial_deploy()
