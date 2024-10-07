
import UIKit
import Firebase

class HomeAdminViewController: UIViewController, UITableViewDataSource, UITableViewDelegate {
    
    @IBOutlet weak var tableView: UITableView!
    
    var deletedDeviceIDs: [String] = []  //store del dev in array
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        // table view
        tableView.dataSource = self
        tableView.delegate = self
        tableView.register(DeletedDeviceTableViewCell.self, forCellReuseIdentifier: "DeletedDeviceCell")
        // Fetch deleted device IDs from Firebase
        fetchDeletedDeviceIDsFromFirebase()
    }
    
    // MARK: - Firebase
    func fetchDeletedDeviceIDsFromFirebase() {
        let ref = Database.database().reference().child("deletedDevices")
        ref.observeSingleEvent(of: .value, with: { (snapshot) in
            // read
            for child in snapshot.children {
                if let childSnapshot = child as? DataSnapshot,
                   let deviceID = childSnapshot.key as? String {
                    self.deletedDeviceIDs.append(deviceID)
                }
            }
            
            // Refresh
            self.tableView.reloadData()
        }) { (error) in
            print("Error fetching deleted device IDs: \(error.localizedDescription)")
        }
    }
    
    // MARK: - Table view data source
    func tableView(_ tableView: UITableView, numberOfRowsInSection section: Int) -> Int {
        return deletedDeviceIDs.count
    }
    
    func tableView(_ tableView: UITableView, cellForRowAt indexPath: IndexPath) -> UITableViewCell {
        let cell = tableView.dequeueReusableCell(withIdentifier: "DeletedDeviceCell", for: indexPath) as! DeletedDeviceTableViewCell
        let deviceID = deletedDeviceIDs[indexPath.row]
        cell.deviceIDLabel.text = deviceID
        return cell
    }
    
    // MARK: - Table view delegate
    func tableView(_ tableView: UITableView, didSelectRowAt indexPath: IndexPath) {
        let deviceID = deletedDeviceIDs[indexPath.row]
        showDeletedDeviceDetails(deviceID: deviceID)
    }
    
    func showDeletedDeviceDetails(deviceID: String) {
        let ref = Database.database().reference().child("deletedDevices").child(deviceID)
        ref.observeSingleEvent(of: .value, with: { (snapshot) in
            if let deviceData = snapshot.value as? [String: Any] {
                let details = self.convertDeviceDataToString(deviceData: deviceData)
                let alertController = UIAlertController(title: "Deleted Device Details", message: details, preferredStyle: .alert)
                let okAction = UIAlertAction(title: "OK", style: .default, handler: nil)
                alertController.addAction(okAction)
                self.present(alertController, animated: true, completion: nil)
            }
        }) { (error) in
            print("Error fetching deleted device details: \(error.localizedDescription)")
        }
    }
    
    func convertDeviceDataToString(deviceData: [String: Any]) -> String {
        
        var details = ""
        for (key, value) in deviceData {
            details += "\(key): \(value)\n"
        }
        return details
    }
}

class DeletedDeviceTableViewCell: UITableViewCell {
    let deviceIDLabel = UILabel() // display the device ID
    
    override init(style: UITableViewCell.CellStyle, reuseIdentifier: String?) {
        super.init(style: style, reuseIdentifier: reuseIdentifier)
        
        // label
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
