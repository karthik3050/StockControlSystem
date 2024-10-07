//
//  RegViewController.swift
//  Stock Control System
//
//  Created by karthik reddy on 06/04/2024.
//

import UIKit
import Firebase
import FirebaseDatabase
import CommonCrypto


private let database = Database.database(url: "https://stockcontrol-f2ff7-default-rtdb.firebaseio.com").reference()
class RegViewController: UIViewController {

    override func viewDidLoad() {
        super.viewDidLoad()
        fname.isSecureTextEntry = true

        // Do any additional setup after loading the view.
    }
    
    override func touchesBegan(_ touches: Set<UITouch>, with event: UIEvent?) {
        mail.resignFirstResponder()
            //   pass.resignFirstResponder()
        fname.resignFirstResponder()
        otp.resignFirstResponder()
    }
    
    @IBOutlet weak var mail: UITextField!
    @IBOutlet weak var fname: UITextField!
    @IBOutlet weak var otp: UITextField!
    
    
    @IBAction func regbutton(_ sender: Any) {
        
        guard let a = mail.text,let c = fname.text,let f = otp.text, !a.isEmpty,!c.isEmpty,!f.isEmpty
                

        else {
            showAlert(title: "Error", message: "Please enter all the details")
            return
            
        }
        
        
        guard let hashedPassword = self.hashPassword(c) 
        else {
            self.showAlert(title: "Error", message: "Failed to hash password")
                                   return
            }
            
        //insert into database
        let object: [String: Any] = [
            "username": a as Any,
            "password": hashedPassword,
            "otp": f as Any,
        ]
        let codeToCheck = otp.text
        let uvcRef = database.child("OTP")
        uvcRef.observeSingleEvent(of: .value) { (snapshot) in
            if snapshot.exists() {
                if snapshot.hasChild(codeToCheck ?? "") {
                    // checking if otp has count of 1
                    database.child("OTP").observeSingleEvent(of: .value) { (snapshot) in
                        if snapshot.exists() {
                            if let value = snapshot.childSnapshot(forPath: codeToCheck ?? "").value as? Int {
                                if value == 0{
                                    database.child("Users").child(a).setValue(object){ (error, _) in
                                        if let error = error {
                                            print("error\(error)")
                                        }else {
                                            let alertController = UIAlertController(
                                                title: "User Registered",
                                                message: "Go back and Login to view/edit",
                                                preferredStyle: .alert
                                            )
                                            let okAction = UIAlertAction(
                                                title: "OK",
                                                style: .default,
                                                handler: nil
                                            )
                                            alertController.addAction(okAction)
                                            self.present(alertController, animated: true, completion: nil)
                                            var rtemp = 1
                                            let userdata = [codeToCheck: rtemp]
                                            database.child("OTP").updateChildValues(userdata) { (error,reference) in
                                                if let error = error {
                                                    print("error") }
                                                else {
                                                    print("success")
                                                }
                                                database.removeAllObservers()
                                            }
                                        }
                                    }
                                }
                                
                                else {
                                    print("loss")
                                    let alertController = UIAlertController(
                                        title: "Registration Unsuccessful",
                                        message: "The given OTP is already registered",
                                        preferredStyle: .alert
                                    )
                                    let okAction = UIAlertAction(
                                        title: "OK",
                                        style: .default,
                                        handler: nil
                                    )
                                    alertController.addAction(okAction)
                                    self.present(alertController, animated: true, completion: nil)
                                    
                                }
                            }
                    }
                }
            } else {
                print("\(String(describing: codeToCheck)) does not exist in the 'OTP' node.")
                print("The 'OTP' node does not exist.")
                let alertController = UIAlertController(
                    title: "Registration Unsuccessful",
                    message: "The given OTP is invalid",
                    preferredStyle: .alert
                )
                let okAction = UIAlertAction(
                    title: "OK",
                    style: .default,
                    handler: nil
                )
                alertController.addAction(okAction)
                self.present(alertController, animated: true, completion: nil)
            }
        }
        else {
            print("The 'OTP' node does not exist.")
            let alertController = UIAlertController(
                title: "Registration Unsuccessful",
                message: "The given OTP is invalid",
                preferredStyle: .alert
            )
            let okAction = UIAlertAction(
                title: "OK",
                style: .default,
                handler: nil
            )
            alertController.addAction(okAction)
            self.present(alertController, animated: true, completion: nil)
        }
    }
}//reg meth
    
    
    // SHA-256 Algorithm sourced from CryptoSwift Library and NIST
    private func hashPassword(_ password: String) -> String? {
            // Convert password to data
            guard let passwordData = password.data(using: .utf8) else {
                return nil
            }
            // Hash password using SHA256
            var hash = [UInt8](repeating: 0, count: Int(CC_SHA256_DIGEST_LENGTH))
            passwordData.withUnsafeBytes {
                _ = CC_SHA256($0.baseAddress, CC_LONG(passwordData.count), &hash)
            }
            // Convert hashed data to string
            let hashedString = hash.map { String(format: "%02x", $0) }.joined()
            return hashedString
        }
    
    
    private func showAlert(title: String, message: String) {
        let alertController = UIAlertController(title: title, message: message, preferredStyle: .alert)
        alertController.addAction(UIAlertAction(title: "OK", style: .default, handler: nil))
        present(alertController, animated: true, completion: nil)
    }
    }

