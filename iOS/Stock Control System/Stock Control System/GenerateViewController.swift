import UIKit
import Firebase

class GenerateViewController: UIViewController, UITableViewDataSource, UITableViewDelegate {
    
    @IBOutlet weak var tableView: UITableView!
    
    var deviceIDs: [String] = [] // Array to store device IDs fetched from Firebase
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        // Set up table view
        tableView.dataSource = self
        tableView.delegate = self
        tableView.register(DeviceTableViewCell1.self, forCellReuseIdentifier: "DeviceCell1") // Use a unique identifier
        // Fetch device IDs from Firebase
        fetchDeviceIDsFromFirebase()
    }
    
    // MARK: - Firebase
    func fetchDeviceIDsFromFirebase() {
        let ref = Database.database().reference().child("devices")
        ref.observeSingleEvent(of: .value, with: { (snapshot) in
            // Process the snapshot to extract device IDs
            for child in snapshot.children {
                if let childSnapshot = child as? DataSnapshot,
                   let deviceID = childSnapshot.key as? String {
                    self.deviceIDs.append(deviceID)
                }
            }
            
            // Reload table view once device IDs are fetched
            self.tableView.reloadData()
        }) { (error) in
            print("Error fetching device IDs: \(error.localizedDescription)")
        }
    }
    
    // MARK: - Table view data source
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return deviceIDs.count
    }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let cell = tableView.dequeueReusableCell(withIdentifier: "DeviceCell1", for: indexPath) as! DeviceTableViewCell1 // Use the correct class name
        let deviceID = deviceIDs[indexPath.row]
        cell.deviceIDLabel.text = deviceID
        return cell
    }
    
    // MARK: - Table view delegate
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        let deviceID = deviceIDs[indexPath.row]
        showDeviceDetails(deviceID: deviceID)
    }
    
    func showDeviceDetails(deviceID: String) {
        let ref = Database.database().reference().child("devices").child(deviceID)
        ref.observeSingleEvent(of: .value, with: { (snapshot) in
            if let deviceData = snapshot.value as? [String: Any] {
                let details = self.convertDeviceDataToString(deviceData: deviceData)
                let alertController = UIAlertController(title: "Device Details", message: details, preferredStyle: .alert)
                let okAction = UIAlertAction(title: "OK", style: .default, handler: nil)
                alertController.addAction(okAction)
                self.present(alertController, animated: true, completion: nil)
            }
        }) { (error) in
            print("Error fetching device details: \(error.localizedDescription)")
        }
    }
    
    func convertDeviceDataToString(deviceData: [String: Any]) -> String {
        //deviceData to a formatted string
        var details = ""
        for (key, value) in deviceData {
            details += "\(key): \(value)\n"
        }
        return details
    }
}

class DeviceTableViewCell1: UITableViewCell {
    let deviceIDLabel = UILabel() //device ID
    
    override init(style: UITableViewCell.CellStyle, reuseIdentifier: String?) {
        super.init(style: style, reuseIdentifier: reuseIdentifier)
        
        // Configure the label
        deviceIDLabel.translatesAutoresizingMaskIntoConstraints = false
        contentView.addSubview(deviceIDLabel)
        NSLayoutConstraint.activate([
            deviceIDLabel.leadingAnchor.constraint(equalTo: contentView.leadingAnchor, constant: 16),
            deviceIDLabel.trailingAnchor.constraint(equalTo: contentView.trailingAnchor, constant: -16),
            deviceIDLabel.topAnchor.constraint(equalTo: contentView.topAnchor, constant: 8),
            deviceIDLabel.bottomAnchor.constraint(equalTo: contentView.bottomAnchor, constant: -8)
        ])
    }
    
    required init?(coder aDecoder: NSCoder) {
        fatalError("init(coder:) has not been implemented")
    }
}

