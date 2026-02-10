import ftplib
import sys

try:
    ftp = ftplib.FTP('windows11.aspone.cz')
    ftp.login('EkoBio.org_lordkikin', 'Brzsilpot7!')
    ftp.cwd('/www/scio/logs')
    files = ftp.nlst()
    
    # Get newest log
    log_files = [f for f in files if f.startswith('stdout')]
    if not log_files:
        print("No log files found")
        sys.exit(0)
    
    # Sort by modification time
    files_with_time = []
    for f in log_files:
        try:
            mtime = ftp.voidcmd(f"MDTM {f}").split()[1]
            files_with_time.append((mtime, f))
        except:
            files_with_time.append(("0", f))
            
    latest = sorted(files_with_time, reverse=True)[0][1]
    print(f"Reading {latest}...")
    
    ftp.retrlines(f"RETR {latest}")
    ftp.quit()
except Exception as e:
    print(f"Error: {e}")
