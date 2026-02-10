import ftplib

# FTP Settings
FTP_SERVER = "windows11.aspone.cz"
FTP_USERNAME = "EkoBio.org_lordkikin"
FTP_PASSWORD = "Brzsilpot7!"
REMOTE_ROOT = "/www/scio"

def count_remote_files(ftp, remote_dir):
    count = 0
    try:
        ftp.cwd(remote_dir)
        lines = []
        ftp.retrlines('LIST', lines.append)
        
        for line in lines:
            parts = line.split()
            if not parts: continue
            
            # -rw-r--r-- (file) or drwxr-xr-x (directory)
            is_dir = line.startswith('d')
            name = " ".join(parts[8:])
            
            if is_dir:
                if name not in ['.', '..']:
                    count += count_remote_files(ftp, f"{remote_dir}/{name}")
            else:
                count += 1
    except:
        pass
    return count

def main():
    try:
        ftp = ftplib.FTP(FTP_SERVER)
        ftp.login(FTP_USERNAME, FTP_PASSWORD)
        ftp.set_pasv(True)
        
        total = count_remote_files(ftp, REMOTE_ROOT)
        print(f"Total files in {REMOTE_ROOT} (recursive): {total}")
        
        ftp.quit()
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()
