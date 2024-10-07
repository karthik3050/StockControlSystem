//
//  WriteViewController.swift
//  Stock Control System
//
//  Created by karthik reddy on 06/04/2024.
//

import UIKit
import Firebase

class WriteViewController: UIViewController, BarcodeScannerDelegate {

    override func viewDidLoad() {
        super.viewDidLoad()

        
    }
    
    @IBOutlet weak var dname: UITextField!
    
    @IBOutlet weak var did: UITextField!
    
    @IBOutlet weak var dtype: UITextField!
    
    @IBOutlet weak var location: UITextField!
    
    @IBOutlet weak var quantity: UITextField!
    
    
    @IBOutlet weak var price: UITextField!
    

    @IBAction func write(_ sender: Any) {
        
        guard let deviceName = dname.text, !deviceName.isEmpty,
                      let deviceId = did.text, !deviceId.isEmpty,
                      let deviceType = dtype.text, !deviceType.isEmpty,
                      let loc = location.text, !loc.isEmpty,
                      let quantitytext = quantity.text ,!quantitytext.isEmpty,
                      let quan = Int(quantitytext),
                      let pricetext = price.text ,!pricetext.isEmpty,
                      let pric = Int(pricetext)
                else {
                    // Handle empty fields
                    return
                }
        let dateFormatter = DateFormatter()
            dateFormatter.dateFormat = "dd/MM/yyyy" // Format as per requirement
            let todayDateString = dateFormatter.string(from: Date())

            let deviceData: [String: Any] = [
                "DeviceName": deviceName,
                "DeviceID": deviceId,
                "DeviceType": deviceType,
                "Location": loc,
                "Quantity": quan,
                "Price": pric,
                "PurchaseDate": todayDateString
                ]
                
                
                let ref = Database.database().reference().child("devices")
                
                //child node with a unique key
                let newDeviceRef = ref.child(deviceId)
                
                // Set the data at the generated node
                newDeviceRef.setValue(deviceData) { (error, ref) in
                    if let error = error {
                        print("Error saving data: \(error.localizedDescription)")
                    } else {
                        print("Data saved successfully!")
                        let alertController = UIAlertController(
                            title: "Success",
                            message: "Data Saved Successfully",
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
        dname.text = ""
        did.text = ""
        dtype.text = ""
        location.text = ""
        quantity.text = ""
        price.text = ""
    }
    
    
    @IBAction func barcode(_ sender: Any) {
        
        let vc = self.storyboard?.instantiateViewController(withIdentifier: "camera") as! BarcodeScannerViewController
            vc.delegate = self
            self.present(vc, animated: true, completion: nil)
    }
    
    
    override func touchesBegan(_ touches: Set<UITouch>, with event: UIEvent?) {
        dname.resignFirstResponder()
        did.resignFirstResponder()
        dtype.resignFirstResponder()
        location.resignFirstResponder()
        quantity.resignFirstResponder()
        price.resignFirstResponder()
    }
   
    func didScanBarcode(_ barcode: String) {
        // Print the scanned barcode data
        print("Scanned Barcode Data: \(barcode)")
        
        // Split the scanned barcode string into separate components
        let components = barcode.components(separatedBy: "\n")
        
        // Ensure that there are exactly 5 components
        guard components.count == 5 else {
            print("Invalid barcode format")
            return
        }
        
        // Populate text fields with scanned data
        dname.text = components[0]
        did.text = components[1]
        dtype.text = components[2]
        quantity.text = components[3]
        price.text = components[4]
        
    }


    
}
