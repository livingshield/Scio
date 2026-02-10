import ftplib

# FTP Settings
FTP_SERVER = "windows11.aspone.cz"
FTP_USERNAME = "EkoBio.org_lordkikin"
FTP_PASSWORD = "Brzsilpot7!"
REMOTE_ROOT = "/www/scio"

def list_recursive(ftp, remote_dir, indent=0):
    try:
        ftp.cwd(remote_dir)
        lines = []
        ftp.retrlines('LIST', lines.append)
        
        for line in lines:
            parts = line.split()
            if not parts: continue
            
            name = " ".join(parts[8:])
            is_dir = line.startswith('d')
            
            if is_dir:
                if name not in ['.', '..']:
                    print("  " * indent + f"[D] {name}")
                    list_recursive(ftp, f"{remote_dir}/{name}", indent + 1)
            else:
                size = parts[4]
                print("  " * indent + f"{name} ({size} bytes)")
    except:
        pass

def main():
    try:
        ftp = ftplib.FTP(FTP_SERVER)
        ftp.login(FTP_USERNAME, FTP_PASSWORD)
        ftp.set_pasv(True)
        
        list_recursive(ftp, REMOTE_ROOT)
        
        ftp.quit()
    except Exception as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    main()
