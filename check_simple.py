#!/usr/bin/env python3
"""
Simplified script to check infrastructure
"""

import ftplib
import pymssql

print("="*60)
print("FTP TEST")
print("="*60)
try:
    ftp = ftplib.FTP("windows11.aspone.cz", timeout=30)
    ftp.login("EkoBio.org_lordkikin", "Brzsilpot7!")
    print(f"SUCCESS: Connected to FTP")
    print(f"Current dir: {ftp.pwd()}")
    files = []
    ftp.dir(files.append)
    print(f"Total items in root: {len(files)}")
    print("\nFirst 10 items:")
    for f in files[:10]:
        print(f"  {f}")
    ftp.quit()
except Exception as e:
    print(f"FAILED: {e}")

print("\n" + "="*60)
print("SQL TEST")
print("="*60)
try:
    conn = pymssql.connect(
        server="sql8.aspone.cz",
        user="db4937",
        password="lordkikin",
        database="db4937",
        timeout=30
    )
    print("SUCCESS: Connected to SQL Server")
    cursor = conn.cursor()
    
    cursor.execute("SELECT @@VERSION")
    version = cursor.fetchone()[0]
    print(f"Version: {version[:100]}")
    
    cursor.execute("""
        SELECT COUNT(*) 
        FROM INFORMATION_SCHEMA.TABLES
    """)
    table_count = cursor.fetchone()[0]
    print(f"\nTotal tables: {table_count}")
    
    cursor.execute("""
        SELECT TABLE_NAME
        FROM INFORMATION_SCHEMA.TABLES
        ORDER BY TABLE_NAME
    """)
    tables = cursor.fetchall()
    if tables:
        print("\nTables:")
        for (name,) in tables[:20]:
            print(f"  - {name}")
    else:
        print("\nNo tables found (empty database)")
    
    conn.close()
except Exception as e:
    print(f"FAILED: {e}")

print("\n" + "="*60)
